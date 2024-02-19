namespace Analog.Cli

open System.IO
open System.Threading
open System.Linq
open Spectre.Console
open Spectre.Console.Cli
open FSharp.Control


type Command() =
    inherit AsyncCommand<Settings>()

    override this.Validate(_, settings) =

        let allExist files =
            files |> Array.filter (fun file -> not (File.Exists file)) |> Array.isEmpty

        match settings.Files with
        | files when files.Length = 0 -> ValidationResult.Error "One or more log files must be specified."
        | files when not (allExist files) -> ValidationResult.Error "One or more log files are not found."
        | _ -> ValidationResult.Success()


    override this.ExecuteAsync(_, settings) =
        task {
            let templates = [ Template.Default; Template.Quasar ]

            let template =
                templates |> List.find (fun option -> option.Name = settings.Template)

            let! logs =
                taskSeq {
                    for path in settings.Files |> Seq.filter File.Exists do
                        use stream = File.OpenRead path
                        yield! stream |> Log.ofStream template CancellationToken.None
                }
                |> TaskSeq.toListAsync

            logs.AsQueryable()
            |> Query.filter settings.Filter
            |> Query.sort settings.SortBy
            |> Seq.toList
            |> Table.ofLogs
            |> AnsiConsole.Write

            return 0
        }
