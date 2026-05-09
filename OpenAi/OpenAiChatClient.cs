using System.Net.Http.Json;
using IrControlAiBot.Configuration;

namespace IrControlAiBot.OpenAi;

public sealed class OpenAiChatClient(HttpClient http, IOptions<OpenAiOptions> options)
    : IOpenAiChatClient
{
    private readonly OpenAiOptions _options = options.Value;

    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new ChatRequest(
            Model: _options.Model,
            Messages:
            [
                new ChatMessage("system", _options.SystemPrompt),
                new ChatMessage("user", prompt),
            ]
        );

        using var response = await http.PostAsJsonAsync(
            "chat/completions",
            request,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        var body =
            await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken)
            ?? throw new InvalidOperationException("OpenAI returned an empty response.");

        return body.Choices.FirstOrDefault()?.Message.Content?.Trim()
            ?? throw new InvalidOperationException("OpenAI returned no completion choices.");
    }
}
