using Spectre.Console;
using Spectre.Console.Cli;

namespace Analog.Console.Commands;

public class ScanCommand : AsyncCommand<ScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScanSettings settings)
    {
        await using FileStream stream = File.OpenRead(settings.FilePath);
        BarChart chart = new() { Width = 60, Label = "Severities" };
        int errors = 0;
        int debugs = 0;
        int infos = 0;
        foreach (IReadOnlyDictionary<string, string> log in await Scanner.Scan(stream, CancellationToken.None))
        {
            if (log.TryGetValue("Severity", out string? severity))
            {
                switch (severity)
                {
                    case "[INF]":
                        infos++;
                        break;
                    case "[ERR]":
                        errors++;
                        break;
                    case "[DBG]":
                        debugs++;
                        break;
                }
            }
        }

        chart.AddItem("Error", errors, Color.Red);
        chart.AddItem("Debug", debugs, Color.Yellow);
        chart.AddItem("Info", infos, Color.Blue);
        AnsiConsole.Write(chart);
        return default;
    }
}