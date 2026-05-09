using System.ComponentModel.DataAnnotations;

namespace IrControlAiBot.Configuration;

public sealed class OpenAiOptions
{
    public const string Section = "OpenAi";

    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }

    public string Model { get; init; } = "gpt-4o-mini";

    [Url]
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    public string SystemPrompt { get; init; } =
        "You are a friendly chef. Keep replies under 200 words.";

    [Range(1, 600)]
    public int TimeoutSeconds { get; init; } = 30;
}
