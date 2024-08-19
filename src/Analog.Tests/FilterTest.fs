module Analog.Tests.FilterTest

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
        |> Snapshot.compare
        |> Snapshot.input [| input |]
        |> Snapshot.exec
    | Error error -> invalidOp error

[<Fact>]
let ``Evaluate expression against a log entry`` () =
    let entry = Map["name", "value"]
    let expression = Binary(Identifier("name"), Equal, Literal(String "value"))
    Evaluator.eval expression entry |> should equal true
