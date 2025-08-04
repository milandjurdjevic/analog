module Analog.Filters

open FParsec
open Analog.Logs

type FilterOperator =
    | EqualOperator
    | GreaterOperator
    | LessOperator
    | ContainOperator
    | StartOperator
    | EndOperator
    | AndOperator
    | OrOperator
    | NotOperator

type FilterExpression =
    | LiteralExpression of Literal
    | MemberExpression of string
    | BinaryExpression of FilterExpression * FilterOperator * FilterExpression

[<RequireQualifiedAccess>]
module Filter =
    let eval (expression: FilterExpression) (entry: Entry) =
        let entryMap = Entry.value entry

        let extractValue =
            function
            | LiteralExpression literal -> Some literal
            | MemberExpression memberName -> Map.tryFind memberName entryMap
            | _ -> None

        let extractNotOperand left right =
            match left, right with
            | _, LiteralExpression _ -> left
            | LiteralExpression _, _ -> right
            | _ -> left

        let compare (op: FilterOperator) (lv: Literal) (rv: Literal) =
            match op, lv, rv with
            | EqualOperator, _, _ -> lv = rv
            | GreaterOperator, NumberLiteral l, NumberLiteral r -> l > r
            | GreaterOperator, TimestampLiteral l, TimestampLiteral r -> l > r
            | GreaterOperator, StringLiteral l, StringLiteral r -> l > r
            | LessOperator, NumberLiteral l, NumberLiteral r -> l < r
            | LessOperator, TimestampLiteral l, TimestampLiteral r -> l < r
            | LessOperator, StringLiteral l, StringLiteral r -> l < r
            | ContainOperator, StringLiteral l, StringLiteral r -> l.Contains r
            | StartOperator, StringLiteral l, StringLiteral r -> l.StartsWith r
            | EndOperator, StringLiteral l, StringLiteral r -> l.EndsWith r
            | _ -> false

        let rec eval =
            function
            | LiteralExpression _ -> true
            | MemberExpression memberName -> Map.containsKey memberName entryMap
            | BinaryExpression(l, NotOperator, r) -> eval (extractNotOperand l r) |> not
            | BinaryExpression(l, (AndOperator | OrOperator as op), r) ->
                let lv = eval l

                match op with
                | AndOperator -> if lv then eval r else false
                | OrOperator -> if lv then true else eval r
                | _ -> false
            | BinaryExpression(l, op, r) ->
                match extractValue l, extractValue r with
                | Some lv, Some rv -> compare op lv rv
                | _ -> false

        eval expression

    let private parse =
        let parseFilterLiteralExpression =
            choice
                [ Boolean.parse |>> BooleanLiteral
                  Number.parse |>> Literal.NumberLiteral
                  String.parseQuoted |>> StringLiteral
                  Timestamp.parse |>> TimestampLiteral ]
            |>> LiteralExpression

        let parseFilterMemberExpression =
            let isMemberChar c = isLetter c || isDigit c || c = '_'
            many1Satisfy isMemberChar |>> MemberExpression

        let opp = OperatorPrecedenceParser<FilterExpression, unit, unit>()

        let inline bin op l r = BinaryExpression(l, op, r)

        let createNot expr =
            BinaryExpression(expr, NotOperator, LiteralExpression(StringLiteral ""))

        let parseAtom =
            choice
                [ parseFilterLiteralExpression
                  parseFilterMemberExpression
                  between (pchar '(' >>. spaces) (spaces >>. pchar ')') opp.ExpressionParser ]
            .>> spaces

        let parseNotOrAtom =
            choice [ pchar '!' >>. spaces >>. parseAtom |>> createNot; parseAtom ]
            .>> spaces

        opp.TermParser <- parseNotOrAtom

        let add infix prec op =
            opp.AddOperator(InfixOperator(infix, spaces, prec, Associativity.Left, bin op))

        add "|" 1 OrOperator
        add "&" 2 AndOperator
        add "=" 3 EqualOperator
        add ">" 3 GreaterOperator
        add "<" 3 LessOperator
        add "~" 3 ContainOperator
        add "^" 3 StartOperator
        add "$" 3 EndOperator

        opp.ExpressionParser

    let create expression =
        match run (spaces >>. parse .>> spaces .>> eof) expression with
        | Success(result, _, _) -> Some result
        | Failure _ -> None