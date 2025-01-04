module Analog.Tests.FilterEvaluatorTest

open System
open Xunit
open FsUnit.Xunit
open Analog

[<Fact>]
let ``Evaluate Const filter with Boolean literal`` () =
    let entry = Map.empty<string, Literal>
    let filter = Filter.Const (Literal.Boolean true)
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Field filter with matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.Boolean true ]
    let filter = Filter.Field "key"
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Field filter with non-matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.Boolean true ]
    let filter = Filter.Field "missingKey"
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal false

[<Fact>]
let ``Evaluate Binary Equal filter with matching String literals`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.String "test"),
            Operator.Equal,
            Filter.Const (Literal.String "test")
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Binary NotEqual filter with non-matching Number literals`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.Number 1.0),
            Operator.NotEqual,
            Filter.Const (Literal.Number 2.0)
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Binary GreaterThan filter with Timestamp literals`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.Timestamp (DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))),
            Operator.GreaterThan,
            Filter.Const (Literal.Timestamp (DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)))
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Binary And filter with Boolean literals`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.Boolean true),
            Operator.And,
            Filter.Const (Literal.Boolean true)
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Binary Or filter with one Boolean literal true`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.Boolean false),
            Operator.Or,
            Filter.Const (Literal.Boolean true)
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal true

[<Fact>]
let ``Evaluate Binary Equal filter with mismatched Literal types`` () =
    let entry = Map.empty<string, Literal>
    let filter =
        Filter.Binary(
            Filter.Const (Literal.String "test"),
            Operator.Equal,
            Filter.Const (Literal.Number 42.0)
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal false

[<Fact>]
let ``Evaluate Binary GreaterThan filter with invalid field in entry`` () =
    let entry = Map.ofList [ "key", Literal.Number 10.0 ]
    let filter =
        Filter.Binary(
            Filter.Field "key",
            Operator.GreaterThan,
            Filter.Const (Literal.Number 20.0)
        )
    let result = FilterEvaluator.evaluate entry filter
    result |> should equal false
