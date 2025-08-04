module Analog.Args

open Argu

[<RequireQualifiedAccess>]
type Arg =
    | [<MainCommand; Mandatory; First>] Input of string
    | [<AltCommandLine("-p")>] Pattern of string
    | [<AltCommandLine("-o")>] Output of string
    | [<AltCommandLine("-f")>] Filter of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Input _ -> "input file path."
            | Pattern _ -> "gork pattern."
            | Output _ -> "output file path."
            | Filter _ -> "filter expression."

let parseArg args =
    try
        ArgumentParser.Create<Arg>().ParseCommandLine args |> Result.Ok
    with err ->
        Error err.Message

let private getArg (getter: ParseResults<Arg> -> 'T) (arg: ParseResults<Arg>) = getter arg

let getInputArgs = getArg (fun arg -> arg.GetResults Arg.Input)
let getOutputArg = getArg (fun arg -> arg.TryGetResult Arg.Output)
let getPatternArg = getArg (fun arg -> arg.TryGetResult Arg.Pattern)
let getFilterArg = getArg (fun arg -> arg.TryGetResult Arg.Filter)
