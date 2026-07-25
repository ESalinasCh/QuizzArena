using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal partial class OpenAICompatibleTextGeneration(
    HttpClient httpClient,
    ILogger<OpenAICompatibleTextGeneration> logger
) : ITextGenerationService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
        var targetType = typeof(T).Name;
        LogGenerateStarted(logger, model, targetType);

        if (typeof(T) == typeof(string))
        {
            var response = await GetCompletionAsync(model, prompt, schema: null);
            var content = response.Choices.First().Message.Content.Trim();
            LogGenerateStringSuccess(logger, model);
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

        LogSchemaGenerated(logger, targetType, schema.ToJsonString());

        var baseResponse = await GetCompletionAsync(model, prompt, schema);
        var baseContent = baseResponse.Choices.First().Message.Content;

        try
        {
            var result = JsonSerializer.Deserialize<T>(
                baseContent,
                _caseInsensitiveOptions
            );

            ArgumentNullException.ThrowIfNull(
                result,
                $"Failed to deserialize response to {targetType}"
            );

            LogGenerateStructuredSuccess(logger, targetType, model);
            return result;
        }
        catch (Exception ex)
        {
            LogDeserializationFailed(logger, targetType, baseContent, ex);
            throw;
        }
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
            max_completion_tokens = 4096,
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

        var serializedPayload = JsonSerializer.Serialize(payload);
        LogSendingRequest(logger, model, serializedPayload);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(
                "chat/completions",
                new StringContent(
                    serializedPayload,
                    Encoding.UTF8,
                    "application/json"
                )
            );
        }
        catch (Exception ex)
        {
            LogHttpRequestFailed(logger, model, ex);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            LogHttpResponseError(logger, (int)response.StatusCode, response.ReasonPhrase ?? string.Empty, errorBody);
            response.EnsureSuccessStatusCode();
        }

        var rawContent = await response.Content.ReadAsStringAsync();
        LogResponseReceived(logger, (int)response.StatusCode, rawContent);

        var completionResponse = JsonSerializer.Deserialize<GroqChatCompletionResponse>(
            rawContent,
            _caseInsensitiveOptions
        );

        if (completionResponse == null ||
            completionResponse.Choices.Count == 0 ||
            string.IsNullOrEmpty(completionResponse.Choices[0].Message.Content))
        {
            LogEmptyResponseFromLlm(logger, model);
            throw new InvalidOperationException("No response from LLM provider");
        }

        return completionResponse;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting text generation with model {model} for type {targetType}")]
    public static partial void LogGenerateStarted(ILogger logger, string model, string targetType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully generated string completion using model {model}")]
    public static partial void LogGenerateStringSuccess(ILogger logger, string model);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Generated JSON Schema for type {targetType}: {schema}")]
    public static partial void LogSchemaGenerated(ILogger logger, string targetType, string schema);

    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully deserialized response to structured type {targetType} using model {model}")]
    public static partial void LogGenerateStructuredSuccess(ILogger logger, string targetType, string model);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to deserialize response to {targetType}. Raw content: {rawContent}")]
    public static partial void LogDeserializationFailed(ILogger logger, string targetType, string rawContent, Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sending HTTP POST to chat/completions with model {model}. Payload: {payload}")]
    public static partial void LogSendingRequest(ILogger logger, string model, string payload);

    [LoggerMessage(Level = LogLevel.Error, Message = "HTTP POST request to LLM provider failed for model {model}")]
    public static partial void LogHttpRequestFailed(ILogger logger, string model, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "LLM provider returned error status code {statusCode} ({reasonPhrase}). Response body: {errorBody}")]
    public static partial void LogHttpResponseError(ILogger logger, int statusCode, string reasonPhrase, string errorBody);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Received HTTP response {statusCode}. Content: {rawContent}")]
    public static partial void LogResponseReceived(ILogger logger, int statusCode, string rawContent);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Received empty or null completion response from LLM provider for model {model}")]
    public static partial void LogEmptyResponseFromLlm(ILogger logger, string model);
}
