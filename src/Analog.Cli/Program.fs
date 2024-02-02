module Analog.Cli.Program

open Spectre.Console.Cli

[<EntryPoint>]
let main args =
    let app = CommandApp()
    app.SetDefaultCommand<Analog.Cli.Command>() |> ignore
    app.Run(args)
