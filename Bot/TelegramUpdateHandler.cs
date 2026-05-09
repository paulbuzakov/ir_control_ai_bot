using IrControlAiBot.Bot.Commands;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace IrControlAiBot.Bot;

public sealed class TelegramUpdateHandler(
    BotIdentity identity,
    IEnumerable<ICommandHandler> handlers,
    ILogger<TelegramUpdateHandler> logger
) : IUpdateHandler
{
    private readonly IReadOnlyList<ICommandHandler> _handlers = [.. handlers];

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        if (update.Message is not { Text: { } text } message)
            return;

        var parsed = CommandParser.TryParse(text, identity.Username);
        if (parsed is not { } command)
            return;

        var handler = _handlers.FirstOrDefault(h => h.CanHandle(command.Name));
        if (handler is null)
            return;

        try
        {
            await handler.HandleAsync(botClient, message, command, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Command handler {Handler} threw while processing /{Command}",
                handler.GetType().Name,
                command.Name
            );
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Telegram polling error from {Source}", source);
        return Task.CompletedTask;
    }
}
