using MassTransit;
using MassTransit.Initializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Commands.Generation;
using QuizzArena.DocumentProcessing.Application.Messaging.Events.Generation;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Utils;
using QuizzArena.DocumentProcessing.Infrastructure.Configuration;
using Shared.Contracts;
using Shared.Contracts.DTOs;

// NOTA: TextChunker está declarado como `internal` en QuizzArena.DocumentProcessing.Application.Helpers.
// Si Infrastructure y Application viven en assemblies distintos, hay que exponerlo como `public`
// o agregar [assembly: InternalsVisibleTo("...Infrastructure")] en el proyecto Application,
// o este archivo no compilará.

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Messaging.Consumers.Generation;

public partial class GenerationProcessingConsumer(
    IDocumentChunkRepository documentChunkRepository,
    IEmbeddingService embeddingGenerationService,
    ITextGenerationService textGenerationService,
    IQuizContract quizContract,
    IQuestionContract questionContract,
    ILogger<GenerationProcessingConsumer> logger,
    IOptions<QuizGenerationOptions> quizGenerationOptions
) : IConsumer<GenerationProcessingCommand>
{
    private readonly QuizGenerationOptions _quizGenerationConfig = quizGenerationOptions.Value;

    // ---- Formatos usados para (de)serializar las respuestas estructuradas del LLM ----
    public record QuizGenerationFormat(string Title, string Description, List<QuestionGenerationFormat> Questions);
    public record QuestionGenerationFormat(string Question, List<string> Options, int CorrectAnswer, string Justification, int ValueScore);

    public record QuestionJudgement(float FactualFidelity, float DistractorQuality, float Relevance);
    public record QuizJudgementFormat(List<QuestionJudgement> Evaluations);

    /// <summary>
    /// Arma el prompt de generación de quiz para un único fragmento de contenido (ya acotado en tamaño).
    /// </summary>
    public static string GenerateQuizPrompt(
        string content,
        int numberOfQuestions,
        int minNumberOfOptions = 2,
        int maxNumberOfOptions = 5,
        BloomTaxonomyLevel bloomTaxonomy = BloomTaxonomyLevel.Remember
    )
    {
        string prompt = $"Generate a quiz with {numberOfQuestions} questions based on the following content:\n{content}\n\n" +
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

    /// <summary>
    /// Arma el prompt de evaluación (judge) para las preguntas generadas a partir de un único fragmento,
    /// usando el texto de ese mismo fragmento como fuente de verdad.
    /// </summary>
    public static string GenerateJudgementPrompt(string content, QuizGenerationFormat generatedQuiz)
    {
        string prompt = $"You are an expert AI Judge evaluating multiple-choice questions based on a source text.\n\n" +
            $"--- SOURCE TEXT ---\n{content}\n\n" +
            $"--- QUIZ TO EVALUATE ---\n{System.Text.Json.JsonSerializer.Serialize(generatedQuiz)}\n\n" +
            $"--- EVALUATION CRITERIA ---\n" +
            $"Evaluate every question by assigning a decimal score from 0.0 to 1.0 for each metric:\n" +
            $"1. FactualFidelity: Is the correct answer directly verifiable by the source text?\n" +
            $"2. DistractorQuality: Are incorrect options plausible but indisputably wrong?\n" +
            $"3. Relevance: Does the question evaluate key core concepts from the text?";
        return prompt;
    }

    public async Task Consume(ConsumeContext<GenerationProcessingCommand> context)
    {
        GenerationProcessingCommand command = context.Message;

        try
        {
            LogStarted(logger, command.ProcessingJobId, command.ClassSourceId, command.NumberOfQuestions);

            // Paso 1: obtener todos los chunks indexados de la fuente documental.
            List<DocumentChunk> documentChunks = (await documentChunkRepository.GetChunksByClassSourceIdAsync(command.ClassSourceId)).ToList();
            if (documentChunks.Count == 0)
            {
                throw new InvalidOperationException("No document chunks found for the specified class source.");
            }

            // Paso 2: fragmentar el contenido total en bloques que quepan en una sola consulta al LLM.
            // Cada chunk de la DB se trata como una "sentence" para TextChunker, que las agrupa
            // respetando maxChunkSize sin cortar ninguna a la mitad.
            List<string> chunkContents = documentChunks.Where(chunk => chunk.Content != null).Select(chunk => chunk.Content!).ToList();
            List<string> contentFragments = TextChunker.ChunkList(chunkContents, separator: "\n");

            LogFragmentsCreated(logger, command.ClassSourceId, chunkContents.Count, contentFragments.Count);

            if (contentFragments.Count == 0)
            {
                throw new InvalidOperationException("Text chunking produced no fragments to generate questions from.");
            }

            // Paso 3: repartir la cantidad total de preguntas entre los fragmentos, lo más parejo posible.
            int[] questionsPerFragment = DistributeQuestions(command.NumberOfQuestions, contentFragments.Count);

            // Paso 4: por cada fragmento -> generar, filtrar por índice válido, juzgar y filtrar por umbral.
            List<QuestionGenerationFormat> survivingQuestions = [];
            string? quizTitle = null;
            string? quizDescription = null;

            for (int fragmentIndex = 0; fragmentIndex < contentFragments.Count; fragmentIndex++)
            {
                int questionsToGenerate = questionsPerFragment[fragmentIndex];
                if (questionsToGenerate == 0)
                {
                    LogFragmentSkipped(logger, command.ClassSourceId, fragmentIndex);
                    continue;
                }

                string fragmentContent = contentFragments[fragmentIndex];
                LogFragmentProcessing(logger, command.ClassSourceId, fragmentIndex, contentFragments.Count, questionsToGenerate);

                // 4a. Generar preguntas para este fragmento únicamente.
                string quizPrompt = GenerateQuizPrompt(
                    fragmentContent,
                    questionsToGenerate,
                    command.MinNumberOfOptions,
                    command.MaxNumberOfOptions,
                    command.BloomTaxonomy
                );

                QuizGenerationFormat fragmentQuiz = await textGenerationService.GenerateAsync<QuizGenerationFormat>(
                    _quizGenerationConfig.QuizGenerationModel,
                    quizPrompt
                );

                // 4b. Descartar preguntas con índice de respuesta correcta fuera de rango.
                List<QuestionGenerationFormat> structurallyValidQuestions = fragmentQuiz.Questions
                    .Where(q => q.CorrectAnswer >= 0 && q.CorrectAnswer < q.Options.Count)
                    .ToList();

                int discardedCount = fragmentQuiz.Questions.Count - structurallyValidQuestions.Count;
                if (discardedCount > 0)
                {
                    LogFragmentInvalidQuestionsDiscarded(logger, command.ClassSourceId, fragmentIndex, discardedCount);
                }

                if (structurallyValidQuestions.Count == 0)
                {
                    LogFragmentNoValidQuestions(logger, command.ClassSourceId, fragmentIndex);
                    continue;
                }

                // Asunción: se usa el título/descripción del primer fragmento que produzca preguntas
                // estructuralmente válidas como título/descripción final del quiz completo.
                quizTitle ??= fragmentQuiz.Title;
                quizDescription ??= fragmentQuiz.Description;

                // 4c. Juzgar las preguntas de este fragmento contra el texto del propio fragmento.
                string judgementPrompt = GenerateJudgementPrompt(
                    fragmentContent,
                    fragmentQuiz with { Questions = structurallyValidQuestions }
                );

                QuizJudgementFormat fragmentJudgement = await textGenerationService.GenerateAsync<QuizJudgementFormat>(
                    _quizGenerationConfig.QuizJudgementModel,
                    judgementPrompt
                );

                if (fragmentJudgement.Evaluations.Count != structurallyValidQuestions.Count)
                {
                    LogFragmentJudgementCountMismatch(
                        logger,
                        command.ClassSourceId,
                        fragmentIndex,
                        structurallyValidQuestions.Count,
                        fragmentJudgement.Evaluations.Count
                    );
                }

                // 4d. Alinear evaluaciones con preguntas: faltantes = válidas por defecto, sobrantes = ignoradas.
                List<QuestionGenerationFormat> judgedQuestions = FilterByJudgement(structurallyValidQuestions, fragmentJudgement.Evaluations);

                LogFragmentJudged(logger, command.ClassSourceId, fragmentIndex, structurallyValidQuestions.Count, judgedQuestions.Count);

                survivingQuestions.AddRange(judgedQuestions);
            }

            if (survivingQuestions.Count == 0)
            {
                throw new InvalidOperationException("No valid questions survived generation and judgement across all fragments.");
            }

            LogTotalQuestionsCollected(logger, command.ClassSourceId, survivingQuestions.Count);

            // Paso 5: deduplicar globalmente todas las preguntas sobrevivientes (de todos los fragmentos)
            // por similitud de coseno entre sus embeddings.
            float[][] embeddedQuestions = await embeddingGenerationService.GenerateMultipleEmbeddingsAsync(
                _quizGenerationConfig.QuestionEmbeddingModel,
                survivingQuestions.Select(q => $"Question: {q.Question}\nAnswer: {q.Options[q.CorrectAnswer]}").ToArray()
            );

            List<QuestionGenerationFormat> finalQuestions = DeduplicateBySimilarity(survivingQuestions, embeddedQuestions);

            LogDeduplicated(logger, command.ClassSourceId, survivingQuestions.Count, finalQuestions.Count);

            // Paso 6: persistir preguntas y quiz (misma lógica que antes, sobre la lista final).
            List<Guid> createdQuestionIds = await questionContract.CreateQuestions(
                finalQuestions.Select(q => new QuestionCreationRequestDTO
                {
                    ProcessingJobId = command.ProcessingJobId,
                    Content = q.Question,
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer,
                    Justification = q.Justification,
                }).ToList()
            );

            // quizTitle/quizDescription nunca son null en este punto: si survivingQuestions.Count > 0,
            // necesariamente algún fragmento pasó por la asignación de 4b/4c.
            Guid quizId = await quizContract.CreateQuiz(new QuizCreationRequestDTO
            {
                Id = Guid.NewGuid(),
                Title = quizTitle!,
                Description = quizDescription!,
                Questions = createdQuestionIds.Select((questionId, index) => new QuizQuestionRequestDTO
                {
                    QuestionId = questionId,
                    Position = index + 1,
                    ValueScore = finalQuestions[index].ValueScore,
                }).ToList()
            });

            LogQuizCreated(logger, command.ClassSourceId, quizId, finalQuestions.Count);

            await context.Publish(new GenerationEndingEvent
            {
                ProcessingJobId = command.ProcessingJobId,
                ClassSourceId = command.ClassSourceId,
                DocumentProcessingJobId = command.DocumentProcessingJobId,
                CreateMatch = command.CreateMatch,
                Title = quizTitle!,
                QuestionAmount = finalQuestions.Count,
                QuizId = quizId,
            });

            LogCompleted(logger, command.ClassSourceId, quizId);
        }
        catch (Exception ex)
        {
            LogFailed(logger, ex, command.ClassSourceId, ex.Message);

            await context.Publish(new GenerationFailedEvent
            {

            });

        }
    }

    /// <summary>
    /// Reparte <paramref name="totalQuestions"/> entre <paramref name="fragmentCount"/> fragmentos usando
    /// redondeo acumulativo, de forma que cualquier resto quede distribuido a lo largo de todo el documento
    /// en vez de amontonarse en los primeros fragmentos.
    /// Ejemplo: 10 preguntas / 4 fragmentos -> [2, 3, 2, 3] (suma exacta = 10).
    /// </summary>
    private static int[] DistributeQuestions(int totalQuestions, int fragmentCount)
    {
        int[] distribution = new int[fragmentCount];
        int previousCumulative = 0;

        for (int i = 0; i < fragmentCount; i++)
        {
            int cumulative = (i + 1) * totalQuestions / fragmentCount;
            distribution[i] = cumulative - previousCumulative;
            previousCumulative = cumulative;
        }

        return distribution;
    }

    /// <summary>
    /// Conserva solo las preguntas cuyo puntaje del juez supera el umbral configurado.
    /// Si el juez devolvió menos evaluaciones que preguntas, las preguntas sin puntaje se asumen válidas.
    /// Si devolvió más evaluaciones que preguntas, las evaluaciones sobrantes se ignoran.
    /// </summary>
    private List<QuestionGenerationFormat> FilterByJudgement(
        List<QuestionGenerationFormat> questions,
        List<QuestionJudgement> evaluations
    )
    {
        List<QuestionGenerationFormat> surviving = [];

        for (int i = 0; i < questions.Count; i++)
        {
            if (i >= evaluations.Count)
            {
                // El juez no devolvió puntaje para esta pregunta: se asume válida.
                surviving.Add(questions[i]);
                continue;
            }

            QuestionJudgement evaluation = evaluations[i];
            float averageScore = (evaluation.FactualFidelity + evaluation.DistractorQuality + evaluation.Relevance) / 3;

            if (averageScore >= _quizGenerationConfig.JudgementThreshold)
            {
                surviving.Add(questions[i]);
            }
        }

        return surviving;
    }

    /// <summary>
    /// Elimina preguntas casi-duplicadas (entre todos los fragmentos) según similitud de coseno de sus
    /// embeddings. Se conserva la primera ocurrencia de cada "cluster" de preguntas similares.
    /// </summary>
    private List<QuestionGenerationFormat> DeduplicateBySimilarity(
        List<QuestionGenerationFormat> questions,
        float[][] embeddings
    )
    {
        List<int> acceptedIndexes = [];
        List<QuestionGenerationFormat> accepted = [];

        for (int candidateIndex = 0; candidateIndex < questions.Count; candidateIndex++)
        {
            bool isDuplicate = false;

            foreach (int acceptedIndex in acceptedIndexes)
            {
                double cosineSim = TensorCosineSimilarity.CalculateCosineSimilarity(
                    embeddings[candidateIndex],
                    embeddings[acceptedIndex]
                );

                if (cosineSim >= _quizGenerationConfig.CosineSimilarityThreshold)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                acceptedIndexes.Add(candidateIndex);
                accepted.Add(questions[candidateIndex]);
            }
        }

        return accepted;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Generation started for ProcessingJobId: {ProcessingJobId}, ClassSourceId: {ClassSourceId}. Requested {RequestedQuestions} question(s).")]
    private static partial void LogStarted(ILogger logger, Guid processingJobId, Guid classSourceId, int requestedQuestions);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} split {ChunkCount} document chunk(s) into {FragmentCount} generation fragment(s).")]
    private static partial void LogFragmentsCreated(ILogger logger, Guid classSourceId, int chunkCount, int fragmentCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} fragment {FragmentIndex} skipped (0 questions assigned).")]
    private static partial void LogFragmentSkipped(ILogger logger, Guid classSourceId, int fragmentIndex);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} processing fragment {FragmentIndex}/{TotalFragments}, requesting {QuestionsRequested} question(s).")]
    private static partial void LogFragmentProcessing(ILogger logger, Guid classSourceId, int fragmentIndex, int totalFragments, int questionsRequested);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} fragment {FragmentIndex} discarded {DiscardedCount} question(s) with an out-of-range CorrectAnswer index.")]
    private static partial void LogFragmentInvalidQuestionsDiscarded(ILogger logger, Guid classSourceId, int fragmentIndex, int discardedCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} fragment {FragmentIndex} produced no structurally valid questions; skipping it.")]
    private static partial void LogFragmentNoValidQuestions(ILogger logger, Guid classSourceId, int fragmentIndex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} fragment {FragmentIndex} judge returned {EvaluationCount} score(s) for {QuestionCount} question(s) (count mismatch).")]
    private static partial void LogFragmentJudgementCountMismatch(ILogger logger, Guid classSourceId, int fragmentIndex, int questionCount, int evaluationCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} fragment {FragmentIndex} judged: {SurvivingCount}/{TotalCount} question(s) passed the threshold.")]
    private static partial void LogFragmentJudged(ILogger logger, Guid classSourceId, int fragmentIndex, int totalCount, int survivingCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} collected {TotalQuestions} question(s) across all fragments before deduplication.")]
    private static partial void LogTotalQuestionsCollected(ILogger logger, Guid classSourceId, int totalQuestions);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} deduplication kept {KeptCount}/{OriginalCount} question(s).")]
    private static partial void LogDeduplicated(ILogger logger, Guid classSourceId, int originalCount, int keptCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] ClassSourceId: {ClassSourceId} created quiz {QuizId} with {QuestionCount} question(s).")]
    private static partial void LogQuizCreated(ILogger logger, Guid classSourceId, Guid quizId, int questionCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "[CONSUMER] Generation completed for ClassSourceId: {ClassSourceId}, QuizId: {QuizId}.")]
    private static partial void LogCompleted(ILogger logger, Guid classSourceId, Guid quizId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CONSUMER] Generation failed for ClassSourceId: {ClassSourceId} with error: {ErrorMessage}")]
    private static partial void LogFailed(ILogger logger, Exception exception, Guid classSourceId, string errorMessage);
}
