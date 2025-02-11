module Analog.Core.Tests.FilterTest

open System
open Analog.Core
open Swensen.Unquote
open Xunit

open Filter
open Log

let eval entry filter = eval filter entry

[<Fact>]
let ``Evaluate Const filter with Boolean literal`` () =
    let filter = LiteralExpression(Literal.BooleanLiteral true)
    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Field filter with matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.BooleanLiteral true ] |> Entry
    let filter = MemberExpression "key"
    let result = eval entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Field filter with non-matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.BooleanLiteral true ] |> Entry
    let filter = MemberExpression "missingKey"
    let result = eval entry filter
    test <@ result = false @>

[<Fact>]
let ``Evaluate Binary Equal filter with matching String literals`` () =
    let filter =
        BinaryExpression(
            LiteralExpression(Literal.StringLiteral "test"),
            Operator.EqualOperator,
            LiteralExpression(Literal.StringLiteral "test")
        )

    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary NotEqual filter with non-matching Number literals`` () =
    let filter =
        BinaryExpression(
            LiteralExpression(Literal.NumberLiteral 1.0),
            Operator.NotEqualOperator,
            LiteralExpression(Literal.NumberLiteral 2.0)
        )

    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary GreaterThan filter with Timestamp literals`` () =

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.TimestampLiteral(DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))),
            Operator.GreaterThanOperator,
            LiteralExpression(Literal.TimestampLiteral(DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)))
        )

    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary And filter with Boolean literals`` () =
    let filter =
        BinaryExpression(
            LiteralExpression(Literal.BooleanLiteral true),
            Operator.AndOperator,
            LiteralExpression(Literal.BooleanLiteral true)
        )

    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary Or filter with one Boolean literal true`` () =

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.BooleanLiteral false),
            Operator.OrOperator,
            LiteralExpression(Literal.BooleanLiteral true)
        )

    let result = eval empty filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary Equal filter with mismatched Literal types`` () =
    let filter =
        BinaryExpression(
            LiteralExpression(Literal.StringLiteral "test"),
            Operator.EqualOperator,
            LiteralExpression(Literal.NumberLiteral 42.0)
        )

    let result = eval empty filter
    test <@ result = false @>

[<Fact>]
let ``Evaluate Binary GreaterThan filter with invalid field in entry`` () =
    let entry = Map.ofList [ "key", Literal.NumberLiteral 10.0 ] |> Entry

    let filter =
        BinaryExpression(
            MemberExpression "key",
            Operator.GreaterThanOperator,
            LiteralExpression(Literal.NumberLiteral 20.0)
        )

    let result = eval entry filter
    test <@ result = false @>
