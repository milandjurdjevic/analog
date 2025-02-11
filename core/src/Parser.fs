module Analog.Core.Parser

open System
open FParsec

type private LogLiteral = Log.Literal
type private FilterExpression = Filter.Expression
type private FilterOperator = Filter.Operator

let parse input parser =
    run parser input
    |> function
        | Success(value, _, _) -> Result.Ok value
        | Failure(error, _, _) -> Result.Error error

let dateTimeOffset: Parser<_, unit> =
    restOfLine false
    >>= fun input ->
        try
            DateTimeOffset.Parse input |> preturn
        with err ->
            fail err.Message
    |> attempt

let floatFinite: Parser<_, unit> =
    pfloat
    >>= fun res ->
        if Double.IsInfinity res || Double.IsNaN res then
            fail "Number cannot be infinite or NaN"
        else
            preturn res
    |> attempt

let boolCI: Parser<_, unit> =
    choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]

let stringQuoted: Parser<string, unit> =
    skipChar '\'' >>. manyCharsTill anyChar (skipChar '\'')

let literal: Parser<_, unit> =
    choice
        [ dateTimeOffset |>> LogLiteral.TimestampLiteral
          floatFinite |>> LogLiteral.NumberLiteral
          boolCI |>> LogLiteral.BooleanLiteral
          restOfLine true |>> LogLiteral.StringLiteral ]

let expression: Parser<_, unit> =
    let literalExpression: Parser<_, unit> =
        choice
            [ dateTimeOffset |>> LogLiteral.TimestampLiteral .>> spaces
              floatFinite |>> LogLiteral.NumberLiteral .>> spaces
              boolCI |>> LogLiteral.BooleanLiteral .>> spaces
              stringQuoted |>> LogLiteral.StringLiteral .>> spaces ]
        |>> FilterExpression.LiteralExpression

    let memberExpression: Parser<_, unit> =
        many1Chars (letter <|> digit) |>> FilterExpression.MemberExpression .>> spaces

    let operator = OperatorPrecedenceParser<FilterExpression, _, _>()
    operator.TermParser <- choice [ literalExpression; memberExpression ]

    let bin op left right =
        FilterExpression.BinaryExpression(left, op, right)

    let add = operator.AddOperator
    add (InfixOperator("&", spaces, 1, Associativity.Left, bin FilterOperator.AndOperator))
    add (InfixOperator("|", spaces, 2, Associativity.Left, bin FilterOperator.OrOperator))
    add (InfixOperator(">", spaces, 3, Associativity.None, bin FilterOperator.GreaterThanOperator))
    add (InfixOperator(">=", spaces, 4, Associativity.None, bin FilterOperator.GreaterThanOrEqualOperator))
    add (InfixOperator("<", spaces, 5, Associativity.None, bin FilterOperator.LessThanOperator))
    add (InfixOperator("<=", spaces, 6, Associativity.None, bin FilterOperator.LessThanOrEqualOperator))
    add (InfixOperator("=", spaces, 7, Associativity.None, bin FilterOperator.EqualOperator))
    add (InfixOperator("<>", spaces, 8, Associativity.None, bin FilterOperator.NotEqualOperator))
    operator.ExpressionParser
