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

    static private LogToChart (logs: Log seq) =
        logs
        |> Seq.countBy (_.Severity)
        |> Seq.map (fun (severity, count) -> BarChartItem(severity.ToString(), count, Severity.toColor severity))
        |> BarChart().AddItems
        |> Panel

    static private FileToLog (files: string seq) =
        taskSeq {
            for file in files do
                use stream = File.OpenRead file
                yield! stream |> Log.ofStream CancellationToken.None Template.basic
        }

    static private LogToTable (logs: Log list) =
        let appendColumns (table: Table) =
            logs
            |> List.tryHead
            |> Option.map (fun log ->
                log.CustomDimensions.Keys
                |> Seq.append [ "Severity"; "Timestamp" ]
                |> Array.ofSeq)
            |> Option.defaultValue Array.empty
            |> table.AddColumns

        let appendRows (table: Table) =
            logs
            |> List.map (fun log ->
                log.CustomDimensions.Values
                |> Seq.map Text
                |> Seq.append
                    [ Text(log.Severity.ToString(), Severity.toColor log.Severity)
                      Text(log.Timestamp.ToString()) ]
                |> Seq.cast<IRenderable>
                |> Array.ofSeq)
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
