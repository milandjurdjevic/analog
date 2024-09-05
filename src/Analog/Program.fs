open System
open System.IO
open Analog
open Argu

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

let eval exp (log: Map<string, string>) =
    log |> Map.map (fun _ v -> v :> obj) |> Filter.Evaluator.eval exp

let printJson check log =
    if check log then
        Print.json log

try
    let arg =
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)

    let template =
        arg.TryGetResult Template
        |> Option.map (fun template -> Template.configuration |> Map.find template)
        |> Option.defaultValue (Template.configuration |> Seq.head |> _.Value)

    let filter = arg.TryGetResult Filter |> Option.map Filter.Parser.parse

    let iter handler =
        let next file =
            use stream = File.OpenRead file
            Extract.stream handler template.Pattern stream

        arg.GetResults File |> Seq.iter next

    match filter with
    | None -> iter Print.json
    | Some flt ->
        match flt with
        | Ok exp -> iter <| (printJson <| eval exp)
        | Error err -> err |> Print.error
with :? ArguParseException as exc ->
    exc.Message |> eprintfn "%s"
