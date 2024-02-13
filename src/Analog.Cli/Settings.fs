namespace Analog.Cli

open Spectre.Console.Cli

type Settings() =
    inherit CommandSettings()

    [<CommandArgument(0, "[FILES]")>]
    member val Files: string array = Array.empty with get, set

    [<CommandOption("-f|--filter <FILTER>")>]
    member val Filter: string = "" with get, set

    [<CommandOption("-s|--sortby <SORT_BY>")>]
    member val SortBy: string = "" with get, set
