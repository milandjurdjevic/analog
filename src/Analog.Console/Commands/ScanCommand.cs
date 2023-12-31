using System.Text.Json;

using JetBrains.Annotations;

using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace Analog.Console.Commands;

[UsedImplicitly]
public class ScanCommand : AsyncCommand<ScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScanSettings settings)
    {
        await using FileStream stream = File.OpenRead(settings.Path);

        foreach (IRenderable element in GetOutput(await Scanner.Scan(stream)))
        {
            AnsiConsole.Write(element);
        }

        return 1;
    }

    private static IEnumerable<IRenderable> GetOutput(IEnumerable<IReadOnlyDictionary<string, string>> logs)
    {
        yield return new Panel(new JsonText(JsonSerializer.Serialize(logs)))
            .Header("Logs")
            .RoundedBorder()
            .BorderColor(Color.Magenta1);
    }
}