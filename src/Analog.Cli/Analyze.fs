module Analog.Cli.Analyze

open System.IO
open System.Threading
open FSharp.Control
open Spectre.Console
open Spectre.Console.Cli
open Spectre.Console.Rendering

type Settings() =
    inherit CommandSettings()

    [<CommandOption("-f|--file <FILE>")>]
    member val Files: string array = Array.empty with get, set


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

    static private LogToChart (logs: Map<string, string> seq) =
        logs
        |> Seq.countBy (fun log -> log["Severity"])
        |> Seq.map (fun (severity, count) -> BarChartItem(severity.ToString(), count))
        |> BarChart().AddItems
        |> Panel

    static private FileToLog (files: string seq) =
        taskSeq {
            for file in files do
                use stream = File.OpenRead file
                yield! stream |> Log.ofStream CancellationToken.None Template.basic
        }

    static private LogToTable (logs: Map<string, string> list) =
        let appendColumns (table: Table) =
            logs
            |> List.tryHead
            |> Option.map (fun log -> log.Keys |> Array.ofSeq)
            |> Option.defaultValue Array.empty
            |> table.AddColumns

        let appendRows (table: Table) =
            logs
            |> List.map (fun log -> log.Values |> Seq.map Text |> Seq.cast<IRenderable> |> Array.ofSeq)
            |> List.iter (fun row -> table.AddRow row |> ignore)
            |> fun () -> table


        Table() |> appendColumns |> appendRows


    override this.ExecuteAsync(_, settings) =
        task {
            let! logs = settings.Files |> Command.FileToLog |> TaskSeq.toListAsync
            logs |> Command.LogToChart |> AnsiConsole.Write
            logs |> Command.LogToTable |> AnsiConsole.Write
            return 0
        }
