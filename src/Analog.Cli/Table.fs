module Analog.Cli.Table

open System.Collections.Generic
open Spectre.Console
open Spectre.Console.Rendering

let ofLogs (logs: KeyValuePair<string, string> list list) =
    let appendColumns (table: Table) =
        logs
        |> List.tryHead
        |> Option.map (fun log -> log |> List.map (_.Key) |> Array.ofSeq)
        |> Option.defaultValue Array.empty
        |> table.AddColumns

    let appendRows (table: Table) =
        logs
        |> List.map (fun log ->
            log
            |> List.map (_.Value)
            |> List.map Text
            |> Seq.cast<IRenderable>
            |> Array.ofSeq)
        |> List.iter (fun row -> table.AddRow row |> ignore)

        table

    Table() |> appendColumns |> appendRows
