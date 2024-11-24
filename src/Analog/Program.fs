open System
open Analog
open Analog.Log
open Argu

type Arg =
    | [<MainCommand; Mandatory; First>] File of string
    | [<AltCommandLine("-p"); Unique>] Pattern of string
    | [<AltCommandLine("-f")>] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File _ -> "File path."
            | Pattern _ -> "Grok pattern."
            | Filter _ -> "Data filter."

try
    let args =
        ArgumentParser
            .Create<Arg>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)

    let patterns = Log.Pattern.load "grok.yml"

    let pattern =
        args.TryGetResult Pattern
        |> Option.map (fun pattern -> patterns |> Map.find pattern)
        |> Option.defaultValue (patterns |> Seq.head |> _.Value)

    // TODO: Apply Filter
    // let filter = args.TryGetResult Filter |> Option.map Log.Filter.parse

    args.GetResult File
    |> System.IO.File.ReadAllText
    |> Pattern.extract pattern
    |> Seq.iter (printfn "%A")

with :? ArguParseException as exc ->
    exc.Message |> eprintfn "%s"
