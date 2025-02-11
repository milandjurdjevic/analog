module Analog.Core.Filter

type private LogLiteral = Log.Literal
type private LogEntry = Log.Entry

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
    | LiteralExpression of LogLiteral
    | MemberExpression of string
    | BinaryExpression of Expression * Operator * Expression

type private Evaluation =
    | TemporaryEvaluation of LogLiteral option
    | FinalEvaluation of bool

let eval expression entry =
    let compareLiteral (left: LogLiteral option) (right: LogLiteral option) comparer =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | LogLiteral.StringLiteral _, LogLiteral.StringLiteral _ -> comparer left right
            | LogLiteral.NumberLiteral _, LogLiteral.NumberLiteral _ -> comparer left right
            | LogLiteral.BooleanLiteral _, LogLiteral.BooleanLiteral _ -> comparer left right
            | LogLiteral.TimestampLiteral _, LogLiteral.TimestampLiteral _ -> comparer left right
            | _ -> false
        | _ -> false

    let combineLiteral (left: LogLiteral option) (right: LogLiteral option) combiner =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | LogLiteral.BooleanLiteral left, LogLiteral.BooleanLiteral right -> combiner left right
            | _ -> false
        | _ -> false

    let wrapFinal = LogLiteral.BooleanLiteral >> Some

    let compareEvaluation (left: Evaluation) (right: Evaluation) comparer =
        match left, right with
        | TemporaryEvaluation left, TemporaryEvaluation right -> compareLiteral left right comparer
        | TemporaryEvaluation left, FinalEvaluation right -> compareLiteral left (wrapFinal right) comparer
        | FinalEvaluation left, TemporaryEvaluation right -> compareLiteral (wrapFinal left) right comparer
        | FinalEvaluation left, FinalEvaluation right -> compareLiteral (wrapFinal left) (wrapFinal right) comparer

    let combineEvaluation (left: Evaluation) (right: Evaluation) combiner =
        match left, right with
        | TemporaryEvaluation left, TemporaryEvaluation right -> combineLiteral left right combiner
        | TemporaryEvaluation left, FinalEvaluation right -> combineLiteral left (wrapFinal right) combiner
        | FinalEvaluation left, TemporaryEvaluation right -> combineLiteral (wrapFinal left) right combiner
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

    let rec loop (expression: Expression) (entry: LogEntry) : Evaluation =
        match expression with
        | LiteralExpression right -> right |> Option.Some |> TemporaryEvaluation
        | MemberExpression field -> entry |> Log.literal field |> TemporaryEvaluation
        | BinaryExpression(left, operator, right) ->
            let left = loop left entry
            let right = loop right entry
            evaluateOperator left operator right |> FinalEvaluation

    match loop expression entry with
    | TemporaryEvaluation temp -> temp.IsSome
    | FinalEvaluation final -> final
