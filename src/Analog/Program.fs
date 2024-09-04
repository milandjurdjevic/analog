open System
open System.IO
open System.Text.Json
open Analog
open Argu
open Spectre.Console
open Spectre.Console.Json

type Argument =
    | [<MainCommand; Mandatory; First>] File of string
    | [<AltCommandLine("-t"); Unique>] Template of string
    | [<AltCommandLine("-f")>] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File _ -> "log file path."
            | Template _ -> "log file template."
            | Filter _ -> "log file filter."

try
    let command =
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)

    let template =
        command.TryGetResult Template
        |> Option.map (fun template -> Template.configuration |> Map.find template)
        |> Option.defaultValue (Template.configuration |> Seq.head |> _.Value)

    let render =
        JsonSerializer.Serialize
        >> JsonText
        >> AnsiConsole.Write
        >> AnsiConsole.WriteLine

    let filter =
        match command.TryGetResult Filter with
        | None -> None
        // TODO: Avoid using exceptions.
        | Some value -> Filter.Parser.parse value |> Result.map Some |> Result.defaultWith failwith

    let iter =
        match filter with
        | None -> render
        | Some expression ->
            fun log ->
                let eval = Filter.Evaluator.eval expression

                if eval (log |> Map.map (fun _ v -> v :> obj)) then
                    render log

    let parse = Parser.parse iter template.Pattern
    command.GetResults File |> Seq.iter (fun file -> File.OpenRead file |> parse)
    command |> ignore
with :? ArguParseException as e ->
    eprintfn $"%s{e.Message}"
