namespace Analog.Cli

open Spectre.Console.Cli

type Settings() =
    inherit CommandSettings()

    [<CommandOption("-f|--file <FILE>")>]
    member val Files: string array = Array.empty with get, set
