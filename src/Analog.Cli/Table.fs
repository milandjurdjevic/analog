module Analog.Cli.Table

open System.Collections.Generic
open Microsoft.FSharp.Core
open Spectre.Console
open Spectre.Console.Rendering

let private textOf (highlighting: Map<string, Map<string, Color>>) (dimension: KeyValuePair<string, string>) =
    let color =
        highlighting
        |> Map.tryFind dimension.Key
        |> Option.map (fun colors -> colors |> Map.tryFind dimension.Value |> Option.defaultValue Color.Default)
        |> Option.defaultValue Color.Default

    Text(dimension.Value, color)

let private columnsOf (logs: IReadOnlyDictionary<string, string> seq) =
    logs
    |> Seq.tryHead
    |> Option.map (fun log -> log |> Seq.map (_.Key))
    |> Option.defaultValue List.empty

let private rowsOf (highlighting: Map<string, Map<string, Color>>) (logs: IReadOnlyDictionary<string, string> seq) =
    logs
    |> Seq.map (fun log -> log |> Seq.map (textOf highlighting) |> Seq.cast<IRenderable> |> Array.ofSeq)

let ofLogs (highlighting: Map<string, Map<string, Color>>) (logs: IReadOnlyDictionary<string, string> list) =
    let columns = columnsOf logs
    let rows = rowsOf highlighting logs
    let table = Table()
    columns |> Seq.iter (fun column -> table.AddColumn(column) |> ignore)
    rows |> Seq.iter (fun row -> table.AddRow(row) |> ignore)
    table
