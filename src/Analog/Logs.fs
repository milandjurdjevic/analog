module Analog.Logs

open System
open System.Globalization
open FParsec

type LogLiteral =
    | StringLiteral of string
    | NumberLiteral of float
    | BooleanLiteral of bool
    | TimestampLiteral of DateTimeOffset

type LogEntry = LogEntry of Map<string, LogLiteral>

type LogPattern =
    { Name: string
      Parser: Parser<LogLiteral, unit> }

let pnumber: Parser<LogLiteral, unit> = pfloat |>> NumberLiteral

let pboolean: Parser<LogLiteral, unit> = parseBoolean |>> BooleanLiteral

let ptimestamp: Parser<LogLiteral, unit> =
    let iso8601 =
        regex @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})"
        >>= fun s ->
            match DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None) with
            | true, dt -> preturn dt
            | false, _ -> fail "Invalid timestamp format"

    let commonLog =
        regex @"\d{2}/[A-Za-z]{3}/\d{4}:\d{2}:\d{2}:\d{2} [+-]\d{4}"
        >>= fun s ->
            match
                DateTimeOffset.TryParseExact(
                    s,
                    "dd/MMM/yyyy:HH:mm:ss zzz",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None
                )
            with
            | true, dt -> preturn dt
            | false, _ -> fail "Invalid common log timestamp format"

    let syslog =
        regex @"[A-Za-z]{3} \d{1,2} \d{2}:\d{2}:\d{2}"
        >>= fun s ->
            let year = DateTime.Now.Year
            let fullDate = $"{year} {s}"

            match
                DateTimeOffset.TryParseExact(
                    fullDate,
                    "yyyy MMM d HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal
                )
            with
            | true, dt -> preturn dt
            | false, _ -> fail "Invalid syslog timestamp format"

    attempt iso8601 <|> attempt commonLog <|> syslog |>> TimestampLiteral

let pquotedString: Parser<LogLiteral, unit> =
    between (pchar '"') (pchar '"') (manySatisfy ((<>) '"')) |>> StringLiteral

let punquotedString: Parser<LogLiteral, unit> =
    many1Satisfy (fun c -> c <> ' ' && c <> '\t' && c <> '\n' && c <> '\r')
    |>> StringLiteral

let pstring: Parser<LogLiteral, unit> = attempt pquotedString <|> punquotedString

let pipAddress: Parser<LogLiteral, unit> =
    regex @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}" |>> StringLiteral

let pword: Parser<LogLiteral, unit> = regex @"[A-Za-z0-9_\-\.]+" |>> StringLiteral

let pauto: Parser<LogLiteral, unit> =
    attempt ptimestamp
    <|> attempt pnumber
    <|> attempt pboolean
    <|> attempt pipAddress
    <|> pstring

let pnamedField: Parser<LogPattern, unit> =
    pchar '{' >>. many1Chars (noneOf ":}") .>> pchar ':'
    .>>. many1Chars (noneOf "}")
    .>> pchar '}'
    >>= fun (name, typeName) ->
        let parser =
            match typeName.ToLower() with
            | "string" -> pstring
            | "number"
            | "float" -> pnumber
            | "boolean"
            | "bool" -> pboolean
            | "timestamp"
            | "datetime" -> ptimestamp
            | "ip" -> pipAddress
            | "word" -> pword
            | "auto" -> pauto
            | _ -> pauto // default to auto-detection

        preturn { Name = name; Parser = parser }

let psimpleField: Parser<LogPattern, unit> =
    pchar '{' >>. many1Chars (noneOf "}") .>> pchar '}'
    |>> fun name -> { Name = name; Parser = pauto }

let pliteralText: Parser<string, unit> = many1Chars (noneOf "{")

let ppattern: Parser<(string * LogPattern option) list, unit> =
    many (
        (attempt pnamedField |>> fun field -> ("", Some field))
        <|> (attempt psimpleField |>> fun field -> ("", Some field))
        <|> (pliteralText |>> fun text -> (text, None))
    )

let createParser (pattern: string) : Parser<LogEntry, unit> =
    match run ppattern pattern with
    | Success(patternParts, _, _) ->
        let parseElements =
            patternParts
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

        combinedParser |>> fun results -> results |> List.rev |> Map.ofList |> LogEntry

    | Failure(errorMsg, _, _) -> fail $"Invalid pattern: {errorMsg}"

let parseLog (pattern: string) (logLine: string) : Result<LogEntry, string> =
    let parser = createParser pattern

    match run parser logLine with
    | Success(result, _, _) -> Result.Ok result
    | Failure(errorMsg, _, _) -> Result.Error errorMsg

let convertLogLiteralToObject =
    function
    | BooleanLiteral bool -> box bool
    | StringLiteral str -> box str
    | NumberLiteral num -> box num
    | TimestampLiteral ts -> box ts

let wrapLogEntry = LogEntry

let unwrapLogEntry (LogEntry entry) = entry

let convertLogEntryToObject =
    unwrapLogEntry
    >> Map.map (fun _ -> convertLogLiteralToObject)
    >> Map.toSeq
    >> readOnlyDict
    >> box
