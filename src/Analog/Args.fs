namespace Analog

open Argu

[<RequireQualifiedAccess>]
type Args =
    | [<MainCommand; Mandatory; First>] Input of string
    | [<AltCommandLine("-p")>] Pattern of string
    | [<AltCommandLine("-o")>] Output of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Input _ -> "input file path."
            | Pattern _ -> "gork pattern."
            | Output _ -> "output file path."

module Args =
    let parse args =
        try
            ArgumentParser.Create<Args>().ParseCommandLine args |> Result.Ok
        with err ->
            Result.Error err.Message

    let input (result: ParseResults<_>) = result.GetResults Args.Input
    let output (result: ParseResults<_>) = result.TryGetResult Args.Output
    let pattern (result: ParseResults<_>) = result.TryGetResult Args.Pattern
