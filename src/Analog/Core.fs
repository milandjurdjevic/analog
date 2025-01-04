namespace Analog

open System

type Literal =
    | String of string
    | Number of float
    | Boolean of bool
    | Timestamp of DateTimeOffset

type Entry = Map<string, Literal>

type Operator =
    | Equal
    | NotEqual
    | GreaterThan
    | GreaterThanOrEqual
    | LessThan
    | LessThanOrEqual
    | And
    | Or

type Filter =
    | Const of Literal
    | Field of string
    | Binary of Filter * Operator * Filter

module ParserRunner =
    open FParsec

    let run parser =
        run parser
        >> function
            | Success(value, _, _) -> Result.Ok value
            | Failure(error, _, _) -> Result.Error error

    let tryRun parser =
        run parser
        >> function
            | Result.Ok value -> Option.Some value
            | Result.Error _ -> Option.None

module LiteralParser =
    open FParsec

    let number: Parser<_, unit> =
        pfloat
        >>= fun res ->
            if Double.IsInfinity res || Double.IsNaN res then
                fail "Number cannot be infinite or NaN"
            else
                preturn res
        |> attempt
        |>> Literal.Number

    let boolean: Parser<_, unit> =
        choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]
        |>> Literal.Boolean

    let timestamp: Parser<_, unit> =
        restOfLine false
        >>= fun input ->
            try
                DateTimeOffset.Parse input |> preturn |>> Literal.Timestamp
            with err ->
                fail err.Message
        |> attempt

    let string: Parser<_, unit> = restOfLine true |>> Literal.String

    let literal: Parser<_, unit> = choice [ timestamp; number; boolean; string ]

module EntryParser =
    open GrokNet

    type private RawEntry = Map<string, string>

    let create txt =
        try
            Grok txt |> Result.Ok
        with err ->
            Result.Error err.Message

    let value =
        "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}"
        |> Grok

    let private group list (key, value) =
        match list with
        | [] -> [ Map([ key, value ]) ]
        | head :: tail ->
            if head |> Map.containsKey key then
                [ Map([ key, value ]); head ] @ tail
            else
                (head |> Map.add key value) :: tail

    let private parseRaw (entry: RawEntry) =
        entry
        |> Map.map (fun _ -> ParserRunner.tryRun LiteralParser.literal)
        |> Map.filter (fun _ value -> value |> Option.isSome)
        |> Map.map (fun _ value -> value |> Option.get)

    let parse text (grok: Grok) : Entry list =
        grok.Parse text
        |> Seq.map (fun i -> i.Key, i.Value.ToString())
        |> Seq.fold group List.empty<RawEntry>
        |> List.map parseRaw
        |> List.rev

module FilterParser =
    open FParsec

    let string: Parser<_, unit> =
        skipChar '\'' >>. manyCharsTill anyChar (skipChar '\'') |>> String .>> spaces

    let number: Parser<_, unit> = pfloat |>> Literal.Number .>> spaces

    let boolean: Parser<_, unit> = LiteralParser.boolean .>> spaces

    let timestamp: Parser<_, unit> = LiteralParser.timestamp .>> spaces

    let constant: Parser<_, unit> =
        choice [ string; timestamp; number; boolean ] |>> Const

    let field: Parser<_, unit> = many1Chars (letter <|> digit) |>> Field .>> spaces

    let term: Parser<_, unit> = choice [ constant; field ]

    let expression: Parser<_, unit> =
        let precedence = OperatorPrecedenceParser<Filter, _, _>()
        precedence.TermParser <- choice [ constant; field ]
        let add = precedence.AddOperator
        let binary operator left right = Binary(left, operator, right)
        add (InfixOperator("&", spaces, 1, Associativity.Left, binary And))
        add (InfixOperator("|", spaces, 2, Associativity.Left, binary Or))
        add (InfixOperator(">", spaces, 3, Associativity.None, binary GreaterThan))
        add (InfixOperator(">=", spaces, 4, Associativity.None, binary GreaterThanOrEqual))
        add (InfixOperator("<", spaces, 5, Associativity.None, binary LessThan))
        add (InfixOperator("<=", spaces, 6, Associativity.None, binary LessThanOrEqual))
        add (InfixOperator("=", spaces, 7, Associativity.None, binary Equal))
        add (InfixOperator("<>", spaces, 8, Associativity.None, binary NotEqual))
        precedence.ExpressionParser

module FilterEvaluator =

    type private Eval =
        | Temp of Literal option
        | Final of bool

    let private compareLiteral (left: Literal option) (right: Literal option) comparer =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | String _, String _ -> comparer left right
            | Number _, Number _ -> comparer left right
            | Boolean _, Boolean _ -> comparer left right
            | Timestamp _, Timestamp _ -> comparer left right
            | _ -> false
        | _ -> false

    let private combineLiteral (left: Literal option) (right: Literal option) combiner =
        match left, right with
        | Some left, Some right ->
            match left, right with
            | Boolean left, Boolean right -> combiner left right
            | _ -> false
        | _ -> false

    let private wrapFinal = Literal.Boolean >> Option.Some

    let private compareEvaluation (left: Eval) (right: Eval) comparer =
        match left, right with
        | Temp left, Temp right -> compareLiteral left right comparer
        | Temp left, Final right -> compareLiteral left (wrapFinal right) comparer
        | Final left, Temp right -> compareLiteral (wrapFinal left) right comparer
        | Final left, Final right -> compareLiteral (wrapFinal left) (wrapFinal right) comparer

    let private combineEvaluation (left: Eval) (right: Eval) combiner =
        match left, right with
        | Temp left, Temp right -> combineLiteral left right combiner
        | Temp left, Final right -> combineLiteral left (wrapFinal right) combiner
        | Final left, Temp right -> combineLiteral (wrapFinal left) right combiner
        | Final left, Final right -> combineLiteral (wrapFinal left) (wrapFinal right) combiner

    let private evalOperator (left: Eval) (operator: Operator) (right: Eval) =
        match operator with
        | Equal -> compareEvaluation left right (=)
        | NotEqual -> compareEvaluation left right (<>)
        | GreaterThan -> compareEvaluation left right (>)
        | GreaterThanOrEqual -> compareEvaluation left right (>=)
        | LessThan -> compareEvaluation left right (<)
        | LessThanOrEqual -> compareEvaluation left right (<=)
        | And -> combineEvaluation left right (&&)
        | Or -> combineEvaluation left right (||)

    let rec private eval (expression: Filter) (entry: Entry) : Eval =
        match expression with
        | Filter.Const right -> right |> Option.Some |> Eval.Temp
        | Filter.Field field -> entry |> Map.tryFind field |> Eval.Temp
        | Filter.Binary(left, operator, right) ->
            let left = eval left entry
            let right = eval right entry
            evalOperator left operator right |> Eval.Final

    let evaluate entry expression =
        match eval expression entry with
        | Temp temp -> temp |> Option.isSome
        | Final final -> final