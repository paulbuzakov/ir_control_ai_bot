namespace IrControlAiBot.Bot;

public readonly record struct ParsedCommand(string Name, string Arguments);

public static class CommandParser
{
    public static ParsedCommand? TryParse(string text, string botUsername)
    {
        Console.WriteLine($"Parsing command: {botUsername} {text}");

        var trimmed = text.AsSpan().TrimStart();
        if (trimmed.IsEmpty || trimmed[0] != '/')
            return null;

        var spaceIdx = trimmed.IndexOf(' ');
        var head = spaceIdx < 0 ? trimmed : trimmed[..spaceIdx];
        var rest = spaceIdx < 0 ? ReadOnlySpan<char>.Empty : trimmed[(spaceIdx + 1)..].Trim();

        var atIdx = head.IndexOf('@');
        if (atIdx >= 0)
        {
            var target = head[(atIdx + 1)..];
            if (!target.Equals(botUsername, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Command is for a different bot: {target}");
                return null;
            }
            head = head[..atIdx];
        }

        if (head.Length <= 1)
            return null;

        return new ParsedCommand(head[1..].ToString().ToLowerInvariant(), rest.ToString());
    }
}
