using System.Text.Json.Serialization;

namespace IrControlAiBot.OpenAi;

internal sealed record ChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages
);

internal sealed record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

internal sealed record ChatResponse(
    [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice> Choices
);

internal sealed record ChatChoice(
    [property: JsonPropertyName("message")] ChatMessage Message
);
