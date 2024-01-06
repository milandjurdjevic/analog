using System.Text.Json;

using Analog.Core;

using JetBrains.Annotations;

using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Analog.Console.Commands;

[UsedImplicitly]
public class ScanCommand : AsyncCommand<ScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScanSettings settings)
    {
        await using FileStream stream = File.OpenRead(settings.Path);

        await foreach (IReadOnlyDictionary<string, string> log in Scanner.Scan(stream, CancellationToken.None))
        {
            AnsiConsole.Write(new JsonText(JsonSerializer.Serialize(log)));
        }

        return 1;
    }
}