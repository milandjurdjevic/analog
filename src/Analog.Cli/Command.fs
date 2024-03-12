namespace Analog.Cli

open System
open System.IO
open System.Threading
open System.Linq
open System.Linq.Dynamic.Core
open Spectre.Console
open Spectre.Console.Cli
open FSharp.Control

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

module private Queryable =
    let filter (filter: string) (source: IQueryable<'a>) =
        if String.IsNullOrWhiteSpace filter then
            source
        else
            source.Where(filter)

    let sort (sortBy: string) (source: IQueryable<'a>) =
        if String.IsNullOrWhiteSpace sortBy then
            source
        else
            source.OrderBy(sortBy)

type Command() =
    inherit AsyncCommand<Settings>()

    override this.Validate(_, settings) =

        let allExist files =
            files |> Array.filter (fun file -> not (File.Exists file)) |> Array.isEmpty

        match settings.Files with
        | files when files.Length = 0 -> ValidationResult.Error "One or more log files must be specified."
        | files when not (allExist files) -> ValidationResult.Error "One or more log files are not found."
        | _ -> ValidationResult.Success()

    override this.ExecuteAsync(_, settings) =
        task {
            let templates = Template.Import()

            let template =
                templates
                |> Seq.tryFind (fun option -> option.Name = settings.Template)
                |> Option.defaultValue (Seq.head templates)

            let! logs =
                taskSeq {
                    for path in settings.Files |> Seq.filter File.Exists do
                        use stream = File.OpenRead path
                        yield! template.Parse stream CancellationToken.None
                }
                |> TaskSeq.toListAsync

            logs.AsQueryable()
            |> Queryable.filter settings.Filter
            |> Queryable.sort settings.SortBy
            |> Seq.toList
            |> Table.ofLogs
            |> AnsiConsole.Write

            return 0
        }
