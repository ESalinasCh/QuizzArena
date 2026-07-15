using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal class OpenAICompatibleTextGeneration : ITextGenerationService
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenAICompatibleTextGeneration(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private sealed record GroqChatCompletionResponse(
        List<Choice> Choices
    );

    private sealed record Choice(
        Message Message
    );

    private sealed record Message(
        string Role,
        string Content
    );

    public async Task<T> GenerateAsync<T>(string model, string prompt)
    {
        if (typeof(T) == typeof(string))
        {
            var response = await GetCompletionAsync(model, prompt, schema: null);
            var content = response.Choices.First().Message.Content.Trim();
            return (T)(object)content;
        }

        var exporterOptions = new JsonSchemaExporterOptions
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, node) =>
            {
                if (node is JsonObject jsonObject && jsonObject.ContainsKey("properties"))
                {
                    jsonObject["additionalProperties"] = false;
                }
                return node;
            }
        };

        var schema = JsonSerializerOptions.Default
                    .GetJsonSchemaAsNode(typeof(T), exporterOptions)
                    .AsObject();

        schema["type"] = "object";
        schema["additionalProperties"] = false;

        if (schema.ContainsKey("properties") && schema["properties"] is JsonObject properties)
        {
            schema["required"] = new JsonArray(
                properties.Select(p => JsonValue.Create(p.Key)).ToArray()
            );
        }

        var baseResponse = await GetCompletionAsync(model, prompt, schema);
        var baseContent = baseResponse.Choices.First().Message.Content;

        var result = JsonSerializer.Deserialize<T>(
            baseContent,
            _caseInsensitiveOptions
        );

        ArgumentNullException.ThrowIfNull(
            result,
            $"Failed to deserialize response to {typeof(T).Name}"
        );

        return result;
    }

    private async Task<GroqChatCompletionResponse> GetCompletionAsync(
        string model,
        string prompt,
        JsonNode? schema = null)
    {
        var payload = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.2,
            stream = false,
            response_format = schema == null
                ? null
                : new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = typeof(object).Name.ToLowerInvariant(),
                        strict = true,
                        schema
                    }
                }
        };

        var response = await _httpClient.PostAsync(
            "chat/completions",
            new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            )
        );

        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync();

        var completionResponse = JsonSerializer.Deserialize<GroqChatCompletionResponse>(
            rawContent,
            _caseInsensitiveOptions
        );

        if (completionResponse == null ||
            completionResponse.Choices.Count == 0 ||
            string.IsNullOrEmpty(completionResponse.Choices[0].Message.Content))
        {
            throw new InvalidOperationException("No response from LLM provider");
        }

        return completionResponse;
    }

}
