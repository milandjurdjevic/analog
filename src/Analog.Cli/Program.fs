module Analog.Cli.Program

open Spectre.Console.Cli

[<EntryPoint>]
let main args =
    let app = CommandApp()
    app.Configure(fun configuration -> configuration.AddCommand<Read.Command>("read") |> ignore)
    app.Run(args)
