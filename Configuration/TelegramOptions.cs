using System.ComponentModel.DataAnnotations;

namespace IrControlAiBot.Configuration;

public sealed class TelegramOptions
{
    public const string Section = "Telegram";

    [Required(AllowEmptyStrings = false)]
    public required string Token { get; init; }
}
