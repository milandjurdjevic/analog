namespace Analog.Cli

open System.IO
open System.Threading
open Spectre.Console
open Spectre.Console.Cli
open FSharp.Control


type Command() =
    inherit AsyncCommand<Settings>()

    override this.Validate(_, settings) =
        let files = settings.Files |> Array.filter (fun file -> not (File.Exists file))

        if Array.isEmpty files then
            ValidationResult.Success()
        else
            files
            |> String.concat "\n"
            |> fun files -> ValidationResult.Error $"One or more files are not found:\n{files}"


    override this.ExecuteAsync(_, settings) =
        task {
            let template = Template.Default

            let! logs =
                taskSeq {
                    for path in settings.Files |> Seq.filter File.Exists do
                        use stream = File.OpenRead path
                        yield! stream |> Log.ofStream template.Regex CancellationToken.None
                }
                |> TaskSeq.toListAsync

            logs
            |> Log.filter settings.Filter
            |> Table.ofLogs template.Highlighting
            |> AnsiConsole.Write

            return 0
        }
