using Spectre.Console.Cli;

namespace Analog.Console.Commands;

public class ScanSettings : CommandSettings
{
    [CommandArgument(0, "<FILE_PATH>")] public string FilePath { get; set; } = String.Empty;

    [CommandOption("--summarize-severity <SUMMARIZE_SEVERITY>")]
    public bool SummarizeSeverity { get; set; }
}