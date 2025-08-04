[<AutoOpen>]
module Analog.Core

open System
open FParsec

module Timestamp =
    let parse: Parser<_, unit> =
        restOfLine false
        >>= fun input ->
            try
                DateTimeOffset.Parse input |> preturn
            with err ->
                fail err.Message
        |> attempt

module Number =
    let parse: Parser<_, unit> =
        pfloat
        >>= fun res ->
            if Double.IsInfinity res || Double.IsNaN res then
                fail "Number cannot be infinite or NaN"
            else
                preturn res
        |> attempt

module Boolean =
    let parse: Parser<_, unit> =
        choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]

module String =
    let parseQuoted: Parser<_, unit> =
        between (pchar '\'') (pchar '\'') (manyChars (noneOf "'"))
