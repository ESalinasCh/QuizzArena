using MassTransit;
using MassTransit.Initializers;
using Microsoft.Extensions.Options;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers;

public class GenerationRequestConsumer(
    IDocumentChunkRepository documentChunkRepository,
    IEmbeddingService embeddingGenerationService,
    ITextGenerationService textGenerationService,
    ICosineSimilarity cosineSimilarityService,
    IQuizContract quizContract,
    IQuestionContract questionContract,
    IOptions<QuizGenerationOptions> quizGenerationOptions
) : IConsumer<GenerationRequestCommand>
{
    private readonly QuizGenerationOptions quizGenerationConfig = quizGenerationOptions.Value;

    public record QuizGenerationFormat(string Title, string Description, List<QuestionGenerationFormat> Questions);
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
                        $"- A very short summary description of the quiz (between 4 and 10 words)\n" +
                        $"- For each question: the question text, answer options, the correct answer index (0-indexed, starting from 0), justification, and a value score (integer >= 1)\n" +
                        $"- IMPORTANT: The correct answer index must be 0-indexed. For example, if the correct answer is the first option, use index 0; if it's the second option, use index 1, etc.\n" +
                        $"- Most questions should have a value score of 1\n" +
                        $"- Assign higher scores (2 or more) only if you consider the question particularly difficult or important.";

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
            quizGenerationConfig.QuizGenerationModel,
            quizPrompt
        );

        llmQuiz = new QuizGenerationFormat(
            llmQuiz.Title,
            llmQuiz.Description,
            llmQuiz.Questions
                .Where(q => q.CorrectAnswer >= 0 && q.CorrectAnswer < q.Options.Count)
                .ToList()
        );

        if (llmQuiz.Questions.Count == 0)
        {
            throw new InvalidOperationException("No valid questions found after filtering out questions with invalid CorrectAnswer indices.");
        }

        string judgementPrompt = GenerateJudgementPrompt(documentChunks, llmQuiz);
        QuizJudgementFormat llmJudgement = await textGenerationService.GenerateAsync<QuizJudgementFormat>(
            quizGenerationConfig.QuizJudgementModel,
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
            llmQuiz.Description,
            llmQuiz.Questions
                .Zip(averageScore, (question, score) => (question, score))
                .Where(qs => qs.score >= quizGenerationConfig.JudgementThreshold)
                .Select(qs => qs.question)
                .ToList()
        );

        float[][] embeddedQuestions = await embeddingGenerationService.GenerateMultipleEmbeddingsAsync(
            quizGenerationConfig.QuestionEmbeddingModel,
            llmQuiz.Questions.Select(qs => $"Question: {qs.Question}\nAnswer: {qs.Options[qs.CorrectAnswer]}").ToArray()
        );

        List<int> acceptedQuestionsIndexes = [];
        llmQuiz = new QuizGenerationFormat(
            llmQuiz.Title,
            llmQuiz.Description,
            llmQuiz.Questions.Where((_, candidateIndex) =>
            {
                foreach (int acceptedIndex in acceptedQuestionsIndexes)
                {
                    double cosineSim = cosineSimilarityService.CalculateCosineSimilarity(
                        embeddedQuestions[candidateIndex],
                        embeddedQuestions[acceptedIndex]
                    );
                    if (cosineSim >= quizGenerationConfig.CosineSimilarityThreshold)
                    {
                        return false;
                    }
                }
                acceptedQuestionsIndexes.Add(candidateIndex);
                return true;
            }).ToList()
        );

        List<Guid> createdQuestionIds = await questionContract.CreateQuestions(
            llmQuiz.Questions.Select(q => new QuestionCreationRequestDTO
            {
                ProcessingJobId = context.Message.ProcessingJobId,
                Content = q.Question,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Justification = q.Justification,
            }
            ).ToList()
        );

        Guid quizId = await quizContract.CreateQuiz(
            new QuizCreationRequestDTO
            {
                Id = Guid.NewGuid(),
                Title = llmQuiz.Title,
                Description = llmQuiz.Description,
                Questions = createdQuestionIds.Select((questionId, index) => new QuizQuestionRequestDTO
                {
                    QuestionId = questionId,
                    Position = index + 1,
                    ValueScore = llmQuiz.Questions[index].ValueScore,
                }).ToList()
            }
        );

        await context.Publish(new GenerationFinalizeProcessingRequestEvent
        {
            ProcessingJobId = context.Message.ProcessingJobId,
            ClassSourceId = context.Message.ClassSourceId,
            DocumentProcessingJobId = context.Message.DocumentProcessingJobId,
            CreateMatch = context.Message.CreateMatch,
            Title = llmQuiz.Title,
            QuestionAmount = llmQuiz.Questions.Count,
            QuizId = quizId,
        });

    }
}
