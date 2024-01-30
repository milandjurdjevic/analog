module Analog.Cli.Read

open System.IO
open System.Threading
open FSharp.Control
open Spectre.Console
open Spectre.Console.Cli

type Settings() =
    inherit CommandSettings()

    [<CommandOption("-f|--file <FILE>")>]
    member val Files: string array = Array.empty with get, set

let private validate (settings: Settings) =
    let files = settings.Files |> Array.filter (fun file -> not (File.Exists file))

    if Array.isEmpty files then
        ValidationResult.Success()
    else
        files
        |> String.concat "\n"
        |> fun files -> ValidationResult.Error $"One or more files are not found:\n{files}"


let private collect (files: string array) =
    taskSeq {
        for file in files do
            use stream = File.OpenRead file
            yield! stream |> Log.ofStream CancellationToken.None Log.Pattern
    }

let private colorOf (severity: string) =
    match severity with
    | "DBG" -> Color.Purple
    | "INF" -> Color.Green
    | "WRN" -> Color.Yellow
    | "ERR" -> Color.Red
    | _ -> Color.Default

let private chartOf (logs: Log seq) =
    logs
    |> Seq.countBy (_.Severity)
    |> Seq.map (fun (severity, count) -> BarChartItem(severity, count, colorOf severity))
    |> BarChart().AddItems

let private execute (settings: Settings) =
    task {
        do!
            settings.Files
            |> collect
            |> TaskSeq.toArrayAsync
            |> Task.map chartOf
            |> Task.map AnsiConsole.Write

        return Unchecked.defaultof<int>
    }

type Command() =
    inherit AsyncCommand<Settings>()

    override this.Validate(_, settings) = validate settings

    override this.ExecuteAsync(_, settings) = execute settings
