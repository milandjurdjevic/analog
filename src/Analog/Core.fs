namespace Analog.Core

open System
open FParsec
open System.Globalization

module Parser =
    let tryRun parser =
        run parser
        >> function
            | Success(result, _, _) -> Some result
            | Failure _ -> None

    let run parser =
        run parser
        >> function
            | Success(result, _, _) -> Result.Ok result
            | Failure(error, _, _) -> Result.Error error

module NumericParser =
    let finiteDouble: Parser<_, unit> =
        pfloat
        >>= fun res ->
            if Double.IsInfinity res || Double.IsNaN res then
                fail "Number cannot be infinite or NaN"
            else
                preturn res
        |> attempt

module BoolParser =
    let caseInsensitive: Parser<_, unit> =
        choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]

module StringParser =
    let unquoted: Parser<_, unit> =
        many1Satisfy (fun c -> c <> ' ' && c <> '\t' && c <> '\n' && c <> '\r')

    let singleQuoted: Parser<_, unit> =
        between (pchar '\'') (pchar '\'') (manyChars (noneOf "'"))

    let doubleQuoted: Parser<_, unit> =
        between (pchar '"') (pchar '"') (manyChars (noneOf "\""))

module TimestampParser =
    let iso8601: Parser<_, unit> =
        regex @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})"
        >>= fun s ->
            match DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None) with
            | true, dt -> preturn dt
            | false, _ -> fail "Invalid timestamp format"
        |> attempt

    let common: Parser<_, unit> =
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
        |> attempt

    let syslog: Parser<_, unit> =
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
        |> attempt

    let any: Parser<_, unit> =
        restOfLine false
        >>= fun input ->
            try
                DateTimeOffset.Parse input |> preturn
            with err ->
                fail err.Message
        |> attempt
