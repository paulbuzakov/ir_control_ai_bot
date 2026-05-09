using IrControlAiBot.Bot.Commands;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace IrControlAiBot.Bot;

public sealed class TelegramBotService(
    ITelegramBotClient client,
    BotIdentity identity,
    TelegramUpdateHandler updateHandler,
    IEnumerable<ICommandHandler> commandHandlers,
    ILogger<TelegramBotService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await client.GetMe(stoppingToken);
        var username =
            me.Username
            ?? throw new InvalidOperationException("Telegram returned a bot without a username.");
        identity.Initialize(username);
        logger.LogInformation("Bot @{Username} connected.", username);

        var commands = commandHandlers.SelectMany(h => h.Commands).ToArray();
        await client.SetMyCommands(commands, cancellationToken: stoppingToken);

        var receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.Message] };

        await client.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
    }
}
