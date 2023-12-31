using JetBrains.Annotations;

using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace Analog.Console.Commands;

[UsedImplicitly]
public class ScanCommand : AsyncCommand<ScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScanSettings settings)
    {
        await using FileStream stream = File.OpenRead(settings.Path);

        foreach (IRenderable element in GetRenderingElements(await Scanner.Scan(stream)))
        {
            AnsiConsole.Write(element);
        }

        return 1;
    }

    private static IEnumerable<IRenderable> GetRenderingElements(IEnumerable<IReadOnlyDictionary<string, string>> logs)
    {
        yield return new Rule("Logs");

        foreach (IReadOnlyDictionary<string, string> log in logs)
        {
            foreach (Text text in log.Select(pair => new Text(pair.Value)))
            {
                yield return text;
            }
        }
    }
}