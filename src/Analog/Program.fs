open System
open System.IO
open Analog
open Argu

type Argument =
    | [<MainCommand; Mandatory; First>] File of string
    | [<AltCommandLine("-e"); Unique>] Extraction of string
    | [<AltCommandLine("-t")>] Transformation of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | File _ -> "Log file path."
            | Extraction _ -> "Extraction pattern."
            | Transformation _ -> "Transformation expression."

let handle (args: ParseResults<Argument>) =
    let text =
        args.GetResults Argument.File
        |> List.map File.ReadAllText
        |> List.reduce (fun all next -> all + next)

    let extract =
        args.TryGetResult Argument.Extraction
        |> Option.map Extract.init
        |> Option.defaultValue (Extract.def |> Result.Ok)

    let transform =
        args.TryGetResult Argument.Transformation |> Option.map Filter.Parser.parse

    match extract, transform with
    | Ok extract, None -> Result.Ok(extract, None, text)
    | Ok extract, Some transform -> 
        match transform with
        | Ok transform -> Result.Ok(extract, Some transform, text)
        | Error error -> Result.Error error
    | Error errorValue, None -> Result.Error errorValue
    | Error error, Some _ -> Result.Error error

let args =
    try
        ArgumentParser
            .Create<Argument>()
            .ParseCommandLine(Environment.GetCommandLineArgs() |> Array.skip 1)
        |> Result.Ok
    with err ->
        Result.Error err.Message


match args |> Result.bind handle with
| Ok resultValue -> failwith "not implemented"
| Error errorValue -> failwith "not implemented"
