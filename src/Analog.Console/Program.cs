using Analog.Console.Commands;

using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(c => c.AddCommand<ScanCommand>("scan"));
await app.RunAsync(args);