using JetBrains.Annotations;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Analog.Console.Commands;

[UsedImplicitly]
public class ScanCommand : AsyncCommand<ScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ScanSettings settings)
    {
        await using FileStream stream = File.OpenRead(settings.FilePath);
        IEnumerable<Log> logs = (await Scanner.Scan(stream, CancellationToken.None)).ToArray();

        if (settings.SummarizeSeverity)
        {
            SummarizeSeverity(logs);
        }

        AnsiConsole.Write(new Rule("Logs").Centered());
        foreach (Text text in logs.Select(l =>
                     new Text($"[{l.Timestamp:O}] {l.Message}", GetSeverityColor(l.Severity))))
        {
            AnsiConsole.Write(text);
        }

        return 1;
    }

    private static void SummarizeSeverity(IEnumerable<Log> logs)
    {
        AnsiConsole.Write(new Rule("Summary").Centered());
        BarChart chart = new();
        foreach (KeyValuePair<Severity, int> summary in Analyzer.CountSeverity(logs))
        {
            chart.AddItem(summary.Key.ToString(), summary.Value, GetSeverityColor(summary.Key));
        }

        AnsiConsole.Write(chart);
    }

    private static Color GetSeverityColor(Severity severity)
    {
        return severity switch
        {
            Severity.Trace => Color.Yellow3,
            Severity.Debug => Color.DarkMagenta,
            Severity.Information => Color.Green4,
            Severity.Warning => Color.Orange1,
            Severity.Error => Color.DarkRed_1,
            Severity.Critical => Color.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }
}