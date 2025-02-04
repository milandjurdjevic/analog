module Analog.Filter

open Log
open FParsec

type Operator =
    | EqualOperator
    | NotEqualOperator
    | GreaterThanOperator
    | GreaterThanOrEqualOperator
    | LessThanOperator
    | LessThanOrEqualOperator
    | AndOperator
    | OrOperator

type Expression =
    | LiteralExpression of Literal
    | MemberExpression of string
    | BinaryExpression of Expression * Operator * Expression

let expressionStringLiteralParser =
    skipChar '\'' >>. manyCharsTill anyChar (skipChar '\'') |>> StringLiteral
    .>> spaces

let expressionTimestampLiteralParser = timestampLiteralParser .>> spaces
let expressionNumberLiteralParser = numberLiteralParser .>> spaces
let expressionBooleanLiteralParser = boolLiteralParser .>> spaces

let expressionLiteralParser: Parser<_, unit> =
    choice
        [ expressionStringLiteralParser
          expressionTimestampLiteralParser
          expressionNumberLiteralParser
          expressionBooleanLiteralParser ]
    |>> LiteralExpression

let expressionMemberParser: Parser<_, unit> =
    many1Chars (letter <|> digit) |>> MemberExpression .>> spaces

let expressionParser: Parser<_, unit> =
    let builder = OperatorPrecedenceParser<Expression, _, _>()
    builder.TermParser <- choice [ expressionLiteralParser; expressionMemberParser ]
    let bin op left right = BinaryExpression(left, op, right)
    builder.AddOperator(InfixOperator("&", spaces, 1, Associativity.Left, bin AndOperator))
    builder.AddOperator(InfixOperator("|", spaces, 2, Associativity.Left, bin OrOperator))
    builder.AddOperator(InfixOperator(">", spaces, 3, Associativity.None, bin GreaterThanOperator))
    builder.AddOperator(InfixOperator(">=", spaces, 4, Associativity.None, bin GreaterThanOrEqualOperator))
    builder.AddOperator(InfixOperator("<", spaces, 5, Associativity.None, bin LessThanOperator))
    builder.AddOperator(InfixOperator("<=", spaces, 6, Associativity.None, bin LessThanOrEqualOperator))
    builder.AddOperator(InfixOperator("=", spaces, 7, Associativity.None, bin EqualOperator))
    builder.AddOperator(InfixOperator("<>", spaces, 8, Associativity.None, bin NotEqualOperator))
    builder.ExpressionParser

let parse expression =
    match run expressionParser expression with
    | Success(value, _, _) -> Result.Ok value
    | Failure(error, _, _) -> Result.Error error

type Evaluation =
    | LiteralEvaluation of Literal option
    | FinalEvaluation of bool

let evaluate entry expression =
    let compareLiteral (left: Literal option) (right: Literal option) comparer =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | Literal.StringLiteral _, Literal.StringLiteral _ -> comparer left right
            | Literal.NumberLiteral _, Literal.NumberLiteral _ -> comparer left right
            | Literal.BooleanLiteral _, Literal.BooleanLiteral _ -> comparer left right
            | Literal.TimestampLiteral _, Literal.TimestampLiteral _ -> comparer left right
            | _ -> false
        | _ -> false

    let combineLiteral (left: Literal option) (right: Literal option) combiner =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | Literal.BooleanLiteral left, Literal.BooleanLiteral right -> combiner left right
            | _ -> false
        | _ -> false

    let wrapFinal = Literal.BooleanLiteral >> Option.Some

    let compareEvaluation (left: Evaluation) (right: Evaluation) comparer =
        match left, right with
        | LiteralEvaluation left, LiteralEvaluation right -> compareLiteral left right comparer
        | LiteralEvaluation left, FinalEvaluation right -> compareLiteral left (wrapFinal right) comparer
        | FinalEvaluation left, LiteralEvaluation right -> compareLiteral (wrapFinal left) right comparer
        | FinalEvaluation left, FinalEvaluation right -> compareLiteral (wrapFinal left) (wrapFinal right) comparer

    let combineEvaluation (left: Evaluation) (right: Evaluation) combiner =
        match left, right with
        | LiteralEvaluation left, LiteralEvaluation right -> combineLiteral left right combiner
        | LiteralEvaluation left, FinalEvaluation right -> combineLiteral left (wrapFinal right) combiner
        | FinalEvaluation left, LiteralEvaluation right -> combineLiteral (wrapFinal left) right combiner
        | FinalEvaluation left, FinalEvaluation right -> combineLiteral (wrapFinal left) (wrapFinal right) combiner

    let evaluateOperator (left: Evaluation) (operator: Operator) (right: Evaluation) =
        match operator with
        | EqualOperator -> compareEvaluation left right (=)
        | NotEqualOperator -> compareEvaluation left right (<>)
        | GreaterThanOperator -> compareEvaluation left right (>)
        | GreaterThanOrEqualOperator -> compareEvaluation left right (>=)
        | LessThanOperator -> compareEvaluation left right (<)
        | LessThanOrEqualOperator -> compareEvaluation left right (<=)
        | AndOperator -> combineEvaluation left right (&&)
        | OrOperator -> combineEvaluation left right (||)

    let rec loop (expression: Expression) (entry: Entry) : Evaluation =
        match expression with
        | LiteralExpression right -> right |> Option.Some |> LiteralEvaluation
        | MemberExpression field -> entry |> Map.tryFind field |> LiteralEvaluation
        | BinaryExpression(left, operator, right) ->
            let left = loop left entry
            let right = loop right entry
            evaluateOperator left operator right |> FinalEvaluation

    match loop expression entry with
    | LiteralEvaluation temp -> temp.IsSome
    | FinalEvaluation final -> final
