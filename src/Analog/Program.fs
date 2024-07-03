open System.IO
open System.Text.Json
open Analog
open Argu
open Spectre.Console
open Spectre.Console.Json

type Argument =
    | [<AltCommandLine("-f"); Mandatory>] File of string
    | [<AltCommandLine("-t"); Unique>] Template of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File _ -> "log file path."
            | Template _ -> "log file template."

[<EntryPoint>]
let main argv =
    try
        let command = ArgumentParser.Create<Argument>().ParseCommandLine(argv)

        let template =
            command.TryGetResult Template
            |> Option.map (fun template -> Template.configuration |> Map.find template)
            |> Option.defaultValue (Template.configuration |> Seq.head |> _.Value)

        let render = JsonSerializer.Serialize >> JsonText >> AnsiConsole.Write
        let parse = Parser.parse render template.Pattern
        command.GetResults File |> Seq.iter (fun file -> File.OpenRead file |> parse)
        command |> ignore
        0
    with :? ArguParseException as e ->
        eprintfn $"%s{e.Message}"
        1
