namespace Analog.Cli

open System
open Spectre.Console.Cli

type Settings() =
    inherit CommandSettings()

    [<CommandArgument(0, "[FILES]")>]
    member val Files: string array = Array.empty with get, set

    [<CommandOption("-f|--filter <FILTER>")>]
    member val Filter: string = String.Empty with get, set

    [<CommandOption("-s|--sortby <SORT_BY>")>]
    member val SortBy: string = String.Empty with get, set

    [<CommandOption("-t|--template <TEMPLATE>")>]
    member val Template: string = String.Empty with get, set
