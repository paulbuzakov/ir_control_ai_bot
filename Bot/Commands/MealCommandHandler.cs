using IrControlAiBot.OpenAi;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace IrControlAiBot.Bot.Commands;

public sealed class MealCommandHandler(IOpenAiChatClient openAi, ILogger<MealCommandHandler> logger)
    : ICommandHandler
{
    private static readonly Dictionary<string, string> Meals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["breakfast"] = "ЗАВТРАК",
        ["lunch"] = "ОБЕД",
        ["dinner"] = "УЖИН",
        ["snack"] = "ПЕРЕКУС",
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
        return $"""
            SYSTEM:

            Ты — эксперт-диетолог мирового уровня со специализацией:
            - инсулинорезистентность
            - метаболическое здоровье
            - снижение жира без потери мышц
            - клиническая нутрициология

            Твоя задача — составлять оптимальный приём пищи с высокой метаболической эффективностью.

            Правила:
            - Используй только продукты из списка.
            - Не добавляй продукты вне списка.
            - Строго соблюдай калорийность текущего приёма пищи.
            - Не используй сахар и быстрые углеводы.
            - Ответ должен быть кратким, структурированным и практичным.
            - Ответ только на русском языке.
            - Без лишних объяснений.

            USER:

            ДАННЫЕ:

            Пол: мужской
            Вес: 88 кг
            Цель: снижение инсулинорезистентности + снижение жира
            Активность: сидячая работа + 8–12 тыс шагов
            Суточная калорийность: 2000 ккал

            КАЛОРИЙНОСТЬ ПО ПРИЁМАМ ПИЩИ:
            - Завтрак: 500 ккал
            - Обед: 700 ккал
            - Ужин: 600 ккал
            - Перекус: 200 ккал

            ТЕКУЩИЙ ПРИЁМ ПИЩИ:
            {Meals[meal]}

            ДОСТУПНЫЕ ПРОДУКТЫ:

            Белки:
            - Куриная грудка
            - Лосось
            - Скумбрия
            - Форель
            - Яйца
            - Греческий йогурт без сахара

            Овощи:
            - Брокколи
            - Огурцы
            - Помидоры
            - Листовой салат
            - Зелень
            - Авокадо

            Жиры:
            - Оливковое масло Extra Virgin
            - Миндаль
            - Грецкие орехи

            Углеводы:
            - Гречка

            ДОСТУПНЫЕ ДОБАВКИ:
            - Магний (глицинат/цитрат)
            - Омега-3
            - Витамин D3

            ЗАДАЧА:
            Составь ОДИН оптимальный приём пищи строго из списка продуктов.

            ФОРМАТ ОТВЕТА:

            🍽 Блюдо
            Название блюда

            🧾 Ингредиенты
            - продукт — X г
            - продукт — X г
            - продукт — X г

            ⚡ КБЖУ
            - Калории: XXX ккал
            - Белки: X г
            - Жиры: X г
            - Углеводы: X г

            Калорийность должна строго соответствовать лимиту текущего приёма пищи.

            💊 Добавки
            - добавка — дозировка — эффект

            🧠 Метаболический эффект
            - влияние на инсулин
            - влияние на сытость
            - влияние на энергию
            - влияние на жиросжигание
            """;
    }
}
