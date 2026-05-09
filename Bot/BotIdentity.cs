namespace IrControlAiBot.Bot;

public sealed class BotIdentity
{
    private string? _username;

    public string Username =>
        _username
        ?? throw new InvalidOperationException(
            "Bot identity has not been initialized yet."
        );

    public void Initialize(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Bot username must be non-empty.", nameof(username));
        _username = username;
    }
}
