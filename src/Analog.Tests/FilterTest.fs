module Analog.Tests.FilterTest

open Analog.Filter
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Evaluate expression against a log entry`` () =
    let entry = Map["name", "value" :> obj]
    let expression = Binary(Field("name"), Equal, Const(String "value"))
    Evaluator.eval expression entry |> should equal true
