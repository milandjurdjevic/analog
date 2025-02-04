module Analog.Log

open System
open FParsec
open GrokNet

type Literal =
    | StringLiteral of string
    | NumberLiteral of float
    | BooleanLiteral of bool
    | TimestampLiteral of DateTimeOffset

type Entry = Map<string, Literal>

let timestampLiteralParser =
    restOfLine false
    >>= fun input ->
        try
            DateTimeOffset.Parse input |> preturn
        with err ->
            fail err.Message
    |> attempt
    |>> TimestampLiteral

let numberLiteralParser =
    pfloat
    >>= fun res ->
        if Double.IsInfinity res || Double.IsNaN res then
            fail "Number cannot be infinite or NaN"
        else
            preturn res
    |> attempt
    |>> NumberLiteral

let boolLiteralParser =
    choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]
    |>> BooleanLiteral

let stringLiteralParser: Parser<_, unit> = restOfLine true |>> StringLiteral

let literalParser: Parser<_, unit> =
    choice
        [ timestampLiteralParser
          numberLiteralParser
          boolLiteralParser
          stringLiteralParser ]

let createGrok pattern =
    try
        Grok(pattern) |> Result.Ok
    with err ->
        $"Grok initialization failed with error: {err.Message}" |> Result.Error

let parseGrok text (pattern: Grok) =
    try
        let aggregate: GrokResult -> Map<string, string> list =
            Seq.fold
                (fun list item ->
                    match list with
                    | [] -> [ Map([ item.Key, string item.Value ]) ]
                    | head :: tail ->
                        if head |> Map.containsKey item.Key then
                            [ Map([ item.Key, string item.Value ]); head ] @ tail
                        else
                            (head |> Map.add item.Key (string item.Value)) :: tail)
                List.empty
            >> List.rev

        let transform: Map<string, string> list -> Map<string, Literal> list =
            List.map (
                Map.toSeq
                >> Seq.choose (fun (key, value) ->
                    match run literalParser value with
                    | Success(value, _, _) -> Option.Some(key, value)
                    | Failure _ -> Option.None)
                >> Map.ofSeq
            )

        pattern.Parse text |> aggregate |> transform |> Result.Ok

    with err ->
        $"Grok parsing failed with error: {err.Message}" |> Result.Error

let defaultGrok =
    match createGrok "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}" with
    | Result.Ok value -> value
    | Result.Error error -> failwith error
