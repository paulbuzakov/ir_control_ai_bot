namespace IrControlAiBot.OpenAi;

public interface IOpenAiChatClient
{
    Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken);
}
