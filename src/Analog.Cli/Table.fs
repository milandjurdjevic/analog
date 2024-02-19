module Analog.Cli.Table

open System.Collections.Generic
open Microsoft.FSharp.Core
open Spectre.Console
open Spectre.Console.Rendering

let private dimensionToText (dimension: KeyValuePair<string, obj>) =
    Text(dimension.Value.ToString()) :> IRenderable

let private logToRow (log: IReadOnlyDictionary<string, obj>) =
    log |> Seq.map dimensionToText |> Seq.toList

let private bindColumns (columns: string array) (table: Table) = table.AddColumns columns

let private bindRows (rows: IRenderable list list) (table: Table) =
    rows |> List.iter (fun row -> table.AddRow row |> ignore) |> (fun () -> table)

let ofLogs (logs: IReadOnlyDictionary<string, obj> list) =
    let columns =
        logs
        |> Seq.tryHead
        |> Option.map _.Keys
        |> Option.defaultValue Seq.empty
        |> Array.ofSeq

    let rows = logs |> List.map logToRow

    Table() |> bindColumns columns |> bindRows rows
