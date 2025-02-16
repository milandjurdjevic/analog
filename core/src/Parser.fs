module Analog.Core.Parser

open System
open FParsec

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

let logLiteral: Parser<_, unit> =
    choice
        [ dateTimeOffset |>> Log.TimestampLiteral
          floatFinite |>> Log.NumberLiteral
          boolCI |>> Log.BooleanLiteral
          restOfLine true |>> Log.StringLiteral ]

let filterExpression: Parser<_, unit> =
    let literalExpression: Parser<_, unit> =
        choice
            [ dateTimeOffset |>> Log.TimestampLiteral .>> spaces
              floatFinite |>> Log.NumberLiteral .>> spaces
              boolCI |>> Log.BooleanLiteral .>> spaces
              stringQuoted |>> Log.StringLiteral .>> spaces ]
        |>> Filter.Expression.LiteralExpression

    let memberExpression: Parser<_, unit> =
        many1Chars (letter <|> digit) |>> Filter.Expression.MemberExpression .>> spaces

    let operator = OperatorPrecedenceParser<Filter.Expression, _, _>()
    operator.TermParser <- choice [ literalExpression; memberExpression ]

    let bin op left right =
        Filter.Expression.BinaryExpression(left, op, right)

    let add = operator.AddOperator
    add (InfixOperator("&", spaces, 1, Associativity.Left, bin Filter.AndOperator))
    add (InfixOperator("|", spaces, 2, Associativity.Left, bin Filter.OrOperator))
    add (InfixOperator(">", spaces, 3, Associativity.None, bin Filter.GreaterThanOperator))
    add (InfixOperator(">=", spaces, 4, Associativity.None, bin Filter.GreaterThanOrEqualOperator))
    add (InfixOperator("<", spaces, 5, Associativity.None, bin Filter.LessThanOperator))
    add (InfixOperator("<=", spaces, 6, Associativity.None, bin Filter.LessThanOrEqualOperator))
    add (InfixOperator("=", spaces, 7, Associativity.None, bin Filter.EqualOperator))
    add (InfixOperator("<>", spaces, 8, Associativity.None, bin Filter.NotEqualOperator))
    operator.ExpressionParser
