open System
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

try
    let arg =
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)

    let templates = Template.load ()

    arg.TryGetResult Template
    |> Option.map (fun template -> templates |> Map.find template)
    |> Option.defaultValue (templates |> Seq.head |> _.Value)
    |> ignore
    
    // TODO: Handle Log Parsing & Filtering

with :? ArguParseException as exc ->
    exc.Message |> eprintfn "%s"
