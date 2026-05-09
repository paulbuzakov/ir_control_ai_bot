using Telegram.Bot;
using Telegram.Bot.Types;

namespace IrControlAiBot.Bot.Commands;

public interface ICommandHandler
{
    IReadOnlyCollection<BotCommand> Commands { get; }

    bool CanHandle(string command);

    Task HandleAsync(
        ITelegramBotClient client,
        Message message,
        ParsedCommand command,
        CancellationToken cancellationToken
    );
}
