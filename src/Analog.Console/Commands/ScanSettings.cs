using JetBrains.Annotations;

using Spectre.Console.Cli;

namespace Analog.Console.Commands;

[UsedImplicitly]
public class ScanSettings : CommandSettings
{
    [CommandArgument(0, "<PATH>")] public string Path { get; set; } = String.Empty;
}