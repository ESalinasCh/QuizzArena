    using MassTransit;
    using MassTransit.Initializers;
    using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
    using QuizzArena.DocumentProcessing.Application.Ports.Out;
    using QuizzArena.DocumentProcessing.Domain.Entities;
    using QuizzArena.DocumentProcessing.Domain.Enums;

    namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

    internal class GenerationRequestConsumer(
        IDocumentChunkRepository documentChunkRepository,
        IEmbeddingService embeddingGenerationService,
        ITextGenerationService textGenerationService,
        ICosineSimilarity cosineSimilarityService,
        float cosineSimilarityThreshold = 0.92f,
        float judgementThreshold = 0.75f,
        string questionEmbeddingModel = "bge-m3",
        string quizGenerationModel = "qwen2.5:7b-instruct",
        string quizJudgementModel = "llama3.1:8b-instruct-q4_K_M"
    ) : IConsumer<GenerationRequestCommand>
    {
        public record QuizGenerationFormat(string Title, List<QuestionGenerationFormat> Questions);
            public record QuestionGenerationFormat(string Question, List<string> Options, int CorrectAnswer, string Justification, int ValueScore);

        public record QuestionJudgement(float FactualFidelity, float DistractorQuality, float Relevance);
        public record QuizJudgementFormat(List<QuestionJudgement> Evaluations);

    public static string GenerateQuizPrompt(
            IEnumerable<DocumentChunk> documentChunks,
            int numberOfQuestions,
            int minNumberOfOptions = 2,
            int maxNumberOfOptions = 5,
            BloomTaxonomyLevel bloomTaxonomy = BloomTaxonomyLevel.Remember
        )
        {
            string combinedContent = string.Join("\n", documentChunks.Select(chunk => chunk.Content));

            string prompt = $"Generate a quiz with {numberOfQuestions} questions based on the following content:\n{combinedContent}\n\n" +
                            $"Each question should have between {minNumberOfOptions} and {maxNumberOfOptions} answer options.\n" +
                            $"The questions should be designed to assess the following Bloom's Taxonomy level: {bloomTaxonomy}.\n" +
                            $"Please provide the questions in a structured format, including:\n" +
                            $"- A descriptive title for the entire quiz\n" +
                            $"- For each question: the question text, answer options, the correct answer index, justification, and a value score (integer >= 1)\n" +
                            $"- Most questions should have a value score of 1\n" +
                            $"- Assign higher scores (2 or more) only if you consider the question particularly difficult or important\n" +

            return prompt;
        }

    public static string GenerateJudgementPrompt(
        IEnumerable<DocumentChunk> documentChunks,
        QuizGenerationFormat generatedQuiz
    )
    {
        string combinedContent = string.Join("\n", documentChunks.Select(chunk => chunk.Content));
        string prompt = $"You are an expert AI Judge evaluating multiple-choice questions based on a source text.\n\n" +
                        $"--- SOURCE TEXT ---\n{combinedContent}\n\n" +
                        $"--- QUIZ TO EVALUATE ---\n{System.Text.Json.JsonSerializer.Serialize(generatedQuiz)}\n\n" +
                        $"--- EVALUATION CRITERIA ---\n" +
                        $"Evaluate every question by assigning a decimal score from 0.0 to 1.0 for each metric:\n" +
                        $"1. FactualFidelity: Is the correct answer directly verifiable by the source text?\n" +
                        $"2. DistractorQuality: Are incorrect options plausible but indisputably wrong?\n" +
                        $"3. Relevance: Does the question evaluate key core concepts from the text?";
        return prompt;
    }

    public async Task Consume(ConsumeContext<GenerationRequestCommand> context)
        {

            //ProcessingJob processingJob = new ProcessingJob()
            //{
            //    Id = context.Message.ProcessingJobId,
            //    Status = JobStatus.Processing,
            //    CreatedAt = DateTimeOffset.UtcNow,
            //    UpdatedAt = DateTimeOffset.UtcNow,
            //    DocumentProcessingJobs = new List<DocumentProcessingJob>
            //    {
            //        new DocumentProcessingJob()
            //        {
            //            Id = 0, // Change to GUID  (Typo)
            //            DocumentId = context.Message.ClassSourceId,
            //            ProcessingJobId = context.Message.ProcessingJobId
            //        }
            //    }
            //};
            //await procesingJobRepository.CreateAsync(processingJob);

            IEnumerable<DocumentChunk> documentChunks = await documentChunkRepository.GetChunksByClassSourceIdAsync(context.Message.ClassSourceId);
            if (!documentChunks.Any())
            {
                throw new InvalidOperationException("No document chunks found for the specified class source.");
            }


            string quizPrompt = GenerateQuizPrompt(
                documentChunks,
                context.Message.NumberOfQuestions,
                context.Message.MinNumberOfOptions,
                context.Message.MaxNumberOfOptions,
                context.Message.BloomTaxonomy
            );
            QuizGenerationFormat llmQuiz = await textGenerationService.GenerateAsync<QuizGenerationFormat>(
                quizGenerationModel,
                quizPrompt
            );


            string judgementPrompt = GenerateJudgementPrompt(documentChunks, llmQuiz);
            QuizJudgementFormat llmJudgement = await textGenerationService.GenerateAsync<QuizJudgementFormat>(
                quizJudgementModel,
                judgementPrompt
            );

            if (llmJudgement.Evaluations == null || llmJudgement.Evaluations.Count != llmQuiz.Questions.Count)
            {
                throw new InvalidOperationException("The AI judge returned an invalid or mismatched array of scores.");
            }

            float[] averageScore = llmJudgement.Evaluations
                .Select(evaluation => (evaluation.FactualFidelity + evaluation.DistractorQuality + evaluation.Relevance) / 3)
                .ToArray();

            llmQuiz = new QuizGenerationFormat(
                llmQuiz.Title,
                llmQuiz.Questions
                    .Zip(averageScore, (question, score) => (question, score))
                    .Where(qs => qs.score >= judgementThreshold)
                    .Select(qs => qs.question)
                    .ToList()
            );

            float[][] embeddedQuestions = await embeddingGenerationService.GenerateMultipleEmbeddingsAsync(
                questionEmbeddingModel,
                llmQuiz.Questions.Select(qs => $"Question: {qs.Question}\nAnswer: {qs.Options[qs.CorrectAnswer]}").ToArray()
            );

            List<int> acceptedQuestionsIndexes = [];
            llmQuiz = new QuizGenerationFormat(
                llmQuiz.Title,
                llmQuiz.Questions.Where((_, candidateIndex) =>
                {
                    foreach (int acceptedIndex in acceptedQuestionsIndexes)
                    {
                        double cosineSim = cosineSimilarityService.CalculateCosineSimilarity(
                            embeddedQuestions[candidateIndex],
                            embeddedQuestions[acceptedIndex]
                        );
                        if (cosineSim >= cosineSimilarityThreshold)
                        {
                            return false;
                        }
                    }
                    acceptedQuestionsIndexes.Add(candidateIndex);
                    return true;
                }).ToList()
            );


        }
    }
