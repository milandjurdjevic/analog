module Analog.Core.Tests.ParserTest

open Swensen.Unquote
open Xunit

open System

open Analog.Core
open Filter
open Log

let parse text = Parser.expression |> Parser.parse text

[<Fact>]
let ``parse should correctly parse a constant string`` () =
    let actual = parse "'hello'"

    let expected: Result<Expression, string> =
        Result.Ok(LiteralExpression(StringLiteral "hello"))

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a constant number`` () =
    let actual = parse "42.5"

    let expected: Result<Expression, string> =
        Result.Ok(LiteralExpression(NumberLiteral 42.5))

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a constant boolean (true)`` () =
    let actual = parse "true"

    let expected: Result<Expression, string> =
        Result.Ok(LiteralExpression(BooleanLiteral true))

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a constant boolean (false)`` () =
    let actual = parse "false"

    let expected: Result<Expression, string> =
        Result.Ok(LiteralExpression(BooleanLiteral false))

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a field identifier`` () =
    let actual = parse "fieldName"
    let expected: Result<Expression, string> = Result.Ok(MemberExpression "fieldName")
    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a simple binary expression`` () =
    let actual = parse "'FilterValue' = fieldName"

    let expected: Result<Expression, string> =
        Result.Ok(
            BinaryExpression(
                LiteralExpression(StringLiteral "FilterValue"),
                EqualOperator,
                MemberExpression "fieldName"
            )
        )

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a complex binary expression`` () =
    let actual = parse "'FilterValue' = fieldName & 42 > 10"

    let expected: Result<Expression, string> =
        Result.Ok(
            BinaryExpression(
                BinaryExpression(
                    LiteralExpression(StringLiteral "FilterValue"),
                    EqualOperator,
                    MemberExpression "fieldName"
                ),
                AndOperator,
                BinaryExpression(
                    LiteralExpression(NumberLiteral 42.0),
                    GreaterThanOperator,
                    LiteralExpression(NumberLiteral 10.0)
                )
            )
        )

    test <@ actual = expected @>

[<Fact>]
let ``parse should correctly parse a timestamp`` () =
    let actual = parse "2024-01-03T12:34:56+00:00"

    let expected: Result<Expression, string> =
        Result.Ok(LiteralExpression(TimestampLiteral(DateTimeOffset.Parse "2024-01-03T12:34:56+00:00")))

    test <@ actual = expected @>

[<Fact>]
let ``parse should return an error for invalid input`` () =
    match parse "'unterminated string" with
    | Ok _ -> failwith "parse should return an error for invalid input"
    | Error _ -> ()
