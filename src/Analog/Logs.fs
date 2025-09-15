namespace Analog.Logs

open System
open FParsec

open Analog.Core

type LogLiteral =
    | TextLiteral of string
    | NumericLiteral of float
    | BooleanLiteral of bool
    | TimestampLiteral of DateTimeOffset

module LogLiteral =
    let toObj =
        function
        | BooleanLiteral bool -> box bool
        | TextLiteral str -> box str
        | NumericLiteral num -> box num
        | TimestampLiteral ts -> box ts

module LogLiteralParser =

    let numeric: Parser<LogLiteral, unit> =
        NumericParser.finiteDouble |>> NumericLiteral

    let bool: Parser<LogLiteral, unit> = BoolParser.caseInsensitive |>> BooleanLiteral

    let timestamp: Parser<LogLiteral, unit> = TimestampParser.any |>> TimestampLiteral

    let text: Parser<LogLiteral, unit> =
        choice [ StringParser.doubleQuoted; StringParser.unquoted ] |>> TextLiteral

    let parseLiteral: Parser<LogLiteral, unit> =
        choice [ timestamp; numeric; bool; text ]

    let auto: Parser<LogLiteral, unit> = choice [ timestamp; numeric; bool; text ]

type LogToken =
    { Name: string
      Parser: Parser<LogLiteral, unit> }

module LogTokenParser =

    let named: Parser<LogToken, unit> =
        pchar '{' >>. many1Chars (noneOf ":}") .>> pchar ':'
        .>>. many1Chars (noneOf "}")
        .>> pchar '}'
        >>= fun (name, typeName) ->
            let parser =
                match typeName.ToLower() with
                | "txt" -> LogLiteralParser.text
                | "num" -> LogLiteralParser.numeric
                | "bool" -> LogLiteralParser.bool
                | "ts" -> LogLiteralParser.timestamp
                | _ -> LogLiteralParser.auto

            preturn { Name = name; Parser = parser }

    let simple: Parser<LogToken, unit> =
        pchar '{' >>. many1Chars (noneOf "}") .>> pchar '}'
        |>> fun name ->
            { Name = name
              Parser = LogLiteralParser.auto }

type LogPattern = private LogPattern of (string * LogToken option) list

module LogPattern =
    let simple = "[{timestamp:ts}] [{level:txt}] {message:txt}"
    let value (LogPattern pattern) = pattern

module LogPatternParser =
    let value: Parser<LogPattern, unit> =
        many (
            (attempt LogTokenParser.named |>> fun field -> ("", Some field))
            <|> (attempt LogTokenParser.simple |>> fun field -> ("", Some field))
            <|> (many1Chars (noneOf "{") |>> fun text -> (text, None))
        )
        |>> LogPattern

type LogEntry = private Entry of Map<string, LogLiteral>

module LogEntry =
    let value (Entry entry) = entry

    let toObj =
        value >> Map.map (fun _ -> LogLiteral.toObj) >> Map.toSeq >> readOnlyDict >> box

module LogEntryParser =
    let single (pattern: string) : Parser<LogEntry, unit> =
        match run LogPatternParser.value pattern with
        | Success(patternParts, _, _) ->
            let parseElements =
                patternParts
                |> LogPattern.value
                |> List.map (fun (literal, fieldOpt) ->
                    match fieldOpt with
                    | Some field -> field.Parser |>> fun value -> Some(field.Name, value)
                    | None -> skipString literal >>% None)

            let combinedParser =
                parseElements
                |> List.fold
                    (fun acc elem ->
                        acc .>>. elem
                        |>> fun (results, newResult) ->
                            match newResult with
                            | Some result -> result :: results
                            | None -> results)
                    (preturn [])

            combinedParser |>> (List.rev >> Map.ofList >> Entry)

        | Failure(errorMsg, _, _) -> fail $"Invalid pattern: {errorMsg}"

    let list (pattern: string) (text: string) : LogEntry list =
        let lines =
            text.Split([| '\n'; '\r' |], StringSplitOptions.RemoveEmptyEntries)
            |> Array.map (fun line -> line.Trim())
            |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace line))

        match run LogPatternParser.value pattern with
        | Success(patternParts, _, _) ->
            let parseElements =
                patternParts
                |> LogPattern.value
                |> List.map (fun (literal, fieldOpt) ->
                    match fieldOpt with
                    | Some field -> field.Parser |>> fun value -> Some(field.Name, value)
                    | None -> skipString literal >>% None)

            let combinedParser =
                parseElements
                |> List.fold
                    (fun acc elem ->
                        acc .>>. elem
                        |>> fun (results, newResult) ->
                            match newResult with
                            | Some result -> result :: results
                            | None -> results)
                    (preturn [])
                |>> (List.rev >> Map.ofList >> Entry)

            lines
            |> Array.choose (fun line ->
                match run combinedParser line with
                | Success(entry, _, _) -> Some entry
                | Failure _ -> None)
            |> Array.toList

        | Failure _ -> []
