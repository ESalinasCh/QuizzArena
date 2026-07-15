using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

namespace QuizzArena.DocumentProcessing.Tests.Services;

public class OllamaEmbeddingGenerationTests
{
    private const string BadInput = "BAD";

    [Fact]
    public async Task GenerateMultipleEmbeddingsAsync_AllInputsSucceed_ReturnsEmbeddingsWithNoSkips()
    {
        string[] prompts = ["one", "two", "three"];
        OllamaEmbeddingGeneration service = CreateService(inputs => SuccessResponse(inputs));

        EmbeddingBatchResult result = await service.GenerateMultipleEmbeddingsAsync("bge-m3", prompts);

        result.SkippedIndices.Should().BeEmpty();
        result.Embeddings.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateMultipleEmbeddingsAsync_OneDegenerateInput_IsIsolatedAndSkippedWithoutLosingTheOthers()
    {
        string[] prompts = ["good one", BadInput, "good two", "good three"];
        OllamaEmbeddingGeneration service = CreateService(inputs =>
            inputs.Contains(BadInput)
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : SuccessResponse(inputs));

        EmbeddingBatchResult result = await service.GenerateMultipleEmbeddingsAsync("bge-m3", prompts, batchSize: 4);

        result.SkippedIndices.Should().Equal(1);
        result.Embeddings.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateMultipleEmbeddingsAsync_EveryInputDegenerate_SkipsAllAndReturnsNoEmbeddings()
    {
        string[] prompts = [BadInput, BadInput];
        OllamaEmbeddingGeneration service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        EmbeddingBatchResult result = await service.GenerateMultipleEmbeddingsAsync("bge-m3", prompts, batchSize: 2);

        result.SkippedIndices.Should().Equal(0, 1);
        result.Embeddings.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateSingleEmbeddingAsync_OllamaFails_ThrowsInvalidOperationException()
    {
        OllamaEmbeddingGeneration service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Func<Task> act = () => service.GenerateSingleEmbeddingAsync("bge-m3", BadInput);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static OllamaEmbeddingGeneration CreateService(Func<string[], HttpResponseMessage> respond)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(respond))
        {
            BaseAddress = new Uri("http://localhost:11434/")
        };

        return new OllamaEmbeddingGeneration(httpClient, NullLogger<OllamaEmbeddingGeneration>.Instance);
    }

    private static HttpResponseMessage SuccessResponse(string[] inputs) =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                model = "bge-m3",
                embeddings = inputs.Select(i => new float[] { i.Length, 1f, 2f }).ToArray()
            })
        };

    private sealed class StubHttpMessageHandler(Func<string[], HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = request.Content!.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            using JsonDocument doc = JsonDocument.Parse(body);
            string[] inputs = doc.RootElement.GetProperty("input")
                .EnumerateArray()
                .Select(e => e.GetString()!)
                .ToArray();

            return Task.FromResult(respond(inputs));
        }
    }
}
