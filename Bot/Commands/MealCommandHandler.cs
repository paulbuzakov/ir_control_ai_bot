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
            # 🧠 Роль
            Ты — эксперт-диетолог мирового уровня, специализирующийся на:
            - инсулинорезистентности
            - метаболическом здоровье
            - снижении жировой массы без потери мышц
            - клинической нутрициологии

            Твоя задача — на основе заданных данных создать **идеальный приём пищи**.

            ---

            # 👤 Параметры человека
            - Пол: мужской
            - Вес: 88 кг
            - Цель: снижение инсулинорезистентности + снижение жировой массы
            - Уровень активности: сидячая работа + 8–12 тыс шагов в день

            ---

            # ⚡ Суточная калорийность
            - Всего: **2000 ккал**

            ### 📊 Распределение калорий по приёмам пищи:
            - 🍳 Завтрак — **500 ккал**
            - 🍗 Обед — **700 ккал**
            - 🌙 Ужин — **600 ккал**
            - 🍏 Перекус — **200 ккал**

            ---

            # 🎯 Приём пищи
            Тип приёма: {Meals[meal]}
            (завтрак / обед / ужин / перекус)

            ---

            # 🛒 Доступные продукты (используй ТОЛЬКО их)

            ## Белки:
            - Куриная грудка — 1.7 кг
            - Лосось / скумбрия / форель — 1.2 кг
            - Яйца — 29–30 шт
            - Греческий йогурт без сахара — 1.7 кг

            ## Овощи:
            - Брокколи — 1.2 кг
            - Огурцы — 1.2 кг
            - Помидоры — 1.2 кг
            - Листовой салат / зелень — 500 г
            - Авокадо — 6 шт

            ## Жиры:
            - Оливковое масло Extra Virgin — 300 мл
            - Орехи (миндаль/грецкие) — 360 г

            ## Углеводы:
            - Гречка — 600 г

            ---

            # 💊 Доступные добавки
            - Магний (глицинат/цитрат) — 300–400 мг вечером
            - Омега-3 — 1000–2000 мг
            - Витамин D3 — 2000–4000 IU

            ---

            # 📌 Задача
            Составь один оптимальный приём пищи строго из доступных продуктов.

            ---

            # 📤 ФОРМАТ ОТВЕТА (ОБЯЗАТЕЛЬНО СОБЛЮДАТЬ)

            ## 1. 🍽 Блюдо
            - Название блюда

            ---

            ## 2. 🧾 Ингредиенты
            - продукт — граммовка
            - продукт — граммовка
            - продукт — граммовка

            ---

            ## 3. ⚡ КБЖУ
            - Калории: XXX ккал (строго в рамках лимита приёма пищи)
            - Белки: X г
            - Жиры: X г
            - Углеводы: X г

            ---

            ## 4. 💊 Добавки (строго по времени приёма пищи)

            ### 🍳 Завтрак (500 ккал)
            - добавка
            - дозировка
            - эффект

            ### 🍗 Обед (700 ккал)
            - добавка
            - дозировка
            - эффект

            ### 🌙 Ужин (600 ккал)
            - добавка
            - дозировка
            - эффект

            ### 🍏 Перекус (200 ккал)
            - добавка
            - дозировка
            - эффект

            ---

            ## 5. 🧠 Метаболический эффект
            - влияние на инсулин
            - влияние на сытость и энергию
            - влияние на жиросжигание

            ---

            # 🌐 ЯЗЫК И СТИЛЬ (ОБЯЗАТЕЛЬНО)
            - Ответ только на русском языке
            - Сделать форматирование сообщения максимально удобным, минималистичным для восприятия в Telegram
            - Чётко по пунктам
            - Без воды
            - Без лишних объяснений
            - Только практический результат

            ---

            # 🚫 ВАЖНЫЕ ОГРАНИЧЕНИЯ
            - не добавляй продукты вне списка
            - строго соблюдай калорийность каждого приёма пищи
            - не усложняй рецепты
            - приоритет: стабильный инсулин + снижение жира + сытость
            """;
    }
}
