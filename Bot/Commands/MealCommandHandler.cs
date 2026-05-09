using IrControlAiBot.OpenAi;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace IrControlAiBot.Bot.Commands;

public sealed class MealCommandHandler(
    IOpenAiChatClient openAi,
    ILogger<MealCommandHandler> logger
) : ICommandHandler
{
    private static readonly Dictionary<string, string> Meals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["breakfast"] = "Get a breakfast idea",
        ["lunch"] = "Get a lunch idea",
        ["dinner"] = "Get a dinner idea",
    };

    public IReadOnlyCollection<BotCommand> Commands { get; } =
        [.. Meals.Select(kv => new BotCommand { Command = kv.Key, Description = kv.Value })];

    public bool CanHandle(string command) => Meals.ContainsKey(command);

    public async Task HandleAsync(
        ITelegramBotClient client,
        Message message,
        ParsedCommand command,
        CancellationToken cancellationToken
    )
    {
        await client.SendChatAction(
            message.Chat.Id,
            ChatAction.Typing,
            cancellationToken: cancellationToken
        );

        var prompt = BuildPrompt(command.Name, command.Arguments);
        try
        {
            var reply = await openAi.CompleteAsync(prompt, cancellationToken);
            await client.SendMessage(
                message.Chat.Id,
                reply,
                replyParameters: message.MessageId,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "OpenAI request failed for command {Command}", command.Name);
            await client.SendMessage(
                message.Chat.Id,
                "Sorry, the AI request failed.",
                replyParameters: message.MessageId,
                cancellationToken: cancellationToken
            );
        }
    }

    private static string BuildPrompt(string meal, string extras)
    {
        var baseAsk =
            $"Suggest one simple, tasty {meal} idea with a short ingredient list and 3-5 step recipe.";
        return string.IsNullOrWhiteSpace(extras)
            ? baseAsk
            : $"{baseAsk} Additional preferences: {extras}";
    }
}
