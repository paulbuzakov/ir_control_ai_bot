using System.Net.Http.Headers;
using IrControlAiBot.Bot;
using IrControlAiBot.Bot.Commands;
using IrControlAiBot.Configuration;
using IrControlAiBot.OpenAi;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddInMemoryCollection(
    new Dictionary<string, string?>
    {
        [$"{TelegramOptions.Section}:Token"] = Environment.GetEnvironmentVariable(
            "TELEGRAM_BOT_TOKEN"
        ),
        [$"{OpenAiOptions.Section}:ApiKey"] = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
        [$"{OpenAiOptions.Section}:Model"] = Environment.GetEnvironmentVariable("OPENAI_MODEL"),
    }
        .Where(kv => kv.Value is not null)
        .ToDictionary(kv => kv.Key, kv => kv.Value)
);

Console.WriteLine("Configuration:");
foreach (var kv in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}

builder
    .Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection(TelegramOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder
    .Services.AddOptions<OpenAiOptions>()
    .Bind(builder.Configuration.GetSection(OpenAiOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
    return new TelegramBotClient(options.Token);
});

builder.Services.AddHttpClient<IOpenAiChatClient, OpenAiChatClient>(
    (sp, http) =>
    {
        var options = sp.GetRequiredService<IOptions<OpenAiOptions>>().Value;
        http.BaseAddress = new Uri(options.BaseUrl);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            options.ApiKey
        );
        http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }
);

builder.Services.AddSingleton<BotIdentity>();
builder.Services.AddSingleton<ICommandHandler, MealCommandHandler>();
builder.Services.AddSingleton<TelegramUpdateHandler>();
builder.Services.AddHostedService<TelegramBotService>();

await builder.Build().RunAsync();
