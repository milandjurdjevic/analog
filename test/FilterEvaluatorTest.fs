module Analog.Tests.FilterEvaluatorTest

open System
open Analog
open Swensen.Unquote
open Xunit
open Log
open Filter

[<Fact>]
let ``Evaluate Const filter with Boolean literal`` () =
    let entry = Map.empty<string, Literal>
    let filter = LiteralExpression(Literal.BooleanLiteral true)
    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Field filter with matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.BooleanLiteral true ]
    let filter = MemberExpression "key"
    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Field filter with non-matching field in entry`` () =
    let entry = Map.ofList [ "key", Literal.BooleanLiteral true ]
    let filter = MemberExpression "missingKey"
    let result = evaluate entry filter
    test <@ result = false @>

[<Fact>]
let ``Evaluate Binary Equal filter with matching String literals`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.StringLiteral "test"),
            Operator.EqualOperator,
            LiteralExpression(Literal.StringLiteral "test")
        )

    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary NotEqual filter with non-matching Number literals`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.NumberLiteral 1.0),
            Operator.NotEqualOperator,
            LiteralExpression(Literal.NumberLiteral 2.0)
        )

    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary GreaterThan filter with Timestamp literals`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.TimestampLiteral(DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero))),
            Operator.GreaterThanOperator,
            LiteralExpression(Literal.TimestampLiteral(DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)))
        )

    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary And filter with Boolean literals`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.BooleanLiteral true),
            Operator.AndOperator,
            LiteralExpression(Literal.BooleanLiteral true)
        )

    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary Or filter with one Boolean literal true`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.BooleanLiteral false),
            Operator.OrOperator,
            LiteralExpression(Literal.BooleanLiteral true)
        )

    let result = evaluate entry filter
    test <@ result = true @>

[<Fact>]
let ``Evaluate Binary Equal filter with mismatched Literal types`` () =
    let entry = Map.empty<string, Literal>

    let filter =
        BinaryExpression(
            LiteralExpression(Literal.StringLiteral "test"),
            Operator.EqualOperator,
            LiteralExpression(Literal.NumberLiteral 42.0)
        )

    let result = evaluate entry filter
    test <@ result = false @>

[<Fact>]
let ``Evaluate Binary GreaterThan filter with invalid field in entry`` () =
    let entry = Map.ofList [ "key", Literal.NumberLiteral 10.0 ]

    let filter =
        BinaryExpression(MemberExpression "key", Operator.GreaterThanOperator, LiteralExpression(Literal.NumberLiteral 20.0))

    let result = evaluate entry filter
    test <@ result = false @>
