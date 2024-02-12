module Analog.Cli.Tests.QueryTests

open System.Linq
open Analog.Cli
open Xunit

let data =
    [ Map["Severity", "INF"]; Map["Severity", "ERR"] ]
    |> List.map (fun log -> log |> Map.toSeq |> readOnlyDict)
    |> _.AsQueryable()

[<Theory>]
[<InlineData("ERR")>]
[<InlineData("INF")>]
let filter_equals_verified (severity: string) =
    data
    |> Query.filter $"Severity == \"{severity}\""
    |> Seq.head
    |> fun head -> head["Severity"] = severity
    |> Assert.True
