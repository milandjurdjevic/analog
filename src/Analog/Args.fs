namespace Analog.Args

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
            | Input _ -> "input file."
            | Pattern _ -> "extraction pattern."
            | Output _ -> "output file."
            | Filter _ -> "filter expression."

type IArg =
    abstract member Inputs: string list
    abstract member Output: string option
    abstract member Pattern: string option
    abstract member Filter: string option

module Arg =
    let parse args =
        try
            let results = ArgumentParser.Create<Arg>().ParseCommandLine args

            { new IArg with
                member _Ks.Inputs = results.GetResults Arg.Input
                member _.Filter = results.TryGetResult Arg.Filter
                member _.Pattern = results.TryGetResult Arg.Pattern
                member _.Output = results.TryGetResult Arg.Output }
            |> Ok

        with err ->
            Error err.Message
