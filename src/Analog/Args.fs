module Analog.Args

open Argu

[<RequireQualifiedAccess>]
type private Arg =
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

type IArg =
    abstract member Inputs: string list
    abstract member Output: string option
    abstract member Pattern: string option
    abstract member Filter: string option

[<RequireQualifiedAccess>]
module Arg =
    let parse args =
        try
            let results = ArgumentParser.Create<Arg>().ParseCommandLine args

            { new IArg with
                member this.Inputs = results.GetResults Arg.Input
                member this.Filter = results.TryGetResult Arg.Filter
                member this.Pattern = results.TryGetResult Arg.Pattern
                member this.Output = results.TryGetResult Arg.Output } |> Ok

        with err ->
            Error err.Message
