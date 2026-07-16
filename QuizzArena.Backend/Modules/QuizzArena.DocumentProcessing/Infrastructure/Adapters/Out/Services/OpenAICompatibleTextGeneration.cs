using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.Extensions.Logging;
using QuizzArena.DocumentProcessing.Application.Ports.Out;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Services;

internal class OpenAICompatibleTextGeneration : ITextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAICompatibleTextGeneration> _logger;

    private readonly JsonSerializerOptions _caseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenAICompatibleTextGeneration(HttpClient httpClient, ILogger<OpenAICompatibleTextGeneration> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
        _logger.LogInformation("[LLM-Gen] Iniciando generación con modelo: '{Model}' para el tipo '{Type}'", model, typeof(T).Name);

        if (typeof(T) == typeof(string))
        {
            var response = await GetCompletionAsync(model, prompt, schema: null);
            var content = response.Choices.First().Message.Content.Trim();
            return (T)(object)content;
        }

        _logger.LogDebug("[LLM-Gen] Generando esquema JSON estricto para tipo '{Type}'", typeof(T).Name);

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

        _logger.LogDebug("[LLM-Gen] Esquema estructurado construido: {Schema}", schema.ToJsonString());

        var baseResponse = await GetCompletionAsync(model, prompt, schema);
        var baseContent = baseResponse.Choices.First().Message.Content;

        _logger.LogDebug("[LLM-Gen] Intentando deserializar respuesta JSON a '{Type}'", typeof(T).Name);

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

        var serializedPayload = JsonSerializer.Serialize(payload);
        _logger.LogInformation("[LLM-Gen] Enviando POST a {Uri} con Payload: {Payload}",
            _httpClient.BaseAddress != null ? new Uri(_httpClient.BaseAddress, "chat/completions") : "chat/completions",
            serializedPayload);

        var response = await _httpClient.PostAsync(
            "chat/completions",
            new StringContent(
                serializedPayload,
                Encoding.UTF8,
                "application/json"
            )
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("[LLM-Gen] Falló la petición. Código: {StatusCode} ({Reason}). Respuesta del servidor: {ErrorBody}",
                response.StatusCode, response.ReasonPhrase, errorContent);

            response.EnsureSuccessStatusCode(); // Lanzará el HttpRequestException
        }

        var rawContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("[LLM-Gen] Respuesta cruda exitosa recibida del proveedor.");

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
