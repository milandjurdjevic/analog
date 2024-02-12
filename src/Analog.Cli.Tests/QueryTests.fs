module Analog.Cli.Tests.QueryTests

open System.Linq
open Analog.Cli
open Xunit

[<Theory>]
[<InlineData("", 2)>]
[<InlineData(" ", 2)>]
[<InlineData("Severity == \"ERR\"", 1)>]
let filter_givenExpression_expectedCount (expression: string) (count: int) =
    [ Map["Severity", "INF"]; Map["Severity", "ERR"] ]
    |> List.map (fun log -> log |> Map.toSeq |> readOnlyDict)
    |> _.AsQueryable()
    |> Query.filter expression
    |> Seq.length
    |> fun actualCount -> count = actualCount |> Assert.True
