module Analog.Tests.FilterTest

open System
open Analog.Filter
open Xunit
open FsUnit.Xunit

[<Theory>]
[<InlineData("name = 'value'")>]
[<InlineData("has18 = true | age > 17")>]
let ``Parse filter input as an expression`` input =
    match Parser.parse input with
    | Ok value ->
        value.ToString()
        |> Snapshot.cmp
        |> Snapshot.sub (Guid "8b47e98e9ae54126b5a22794428a9e9e")
        |> Snapshot.arg [ input ]
        |> Snapshot.run
    | Error error -> invalidOp error

[<Fact>]
let ``Evaluate expression against a log entry`` () =
    let entry = Map["name", "value" :> obj]
    let expression = Binary(Identifier("name"), Equal, Literal(String "value"))
    Evaluator.next expression entry |> should equal true
