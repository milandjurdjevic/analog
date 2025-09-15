[<AutoOpen>]
module Analog.Core

open System
open FParsec

let parseDateTimeOffset: Parser<_, unit> =
    restOfLine false
    >>= fun input ->
        try
            DateTimeOffset.Parse input |> preturn
        with err ->
            fail err.Message
    |> attempt

let parseNonInfiniteDouble: Parser<_, unit> =
    pfloat
    >>= fun res ->
        if Double.IsInfinity res || Double.IsNaN res then
            fail "Number cannot be infinite or NaN"
        else
            preturn res
    |> attempt

let parseBoolean: Parser<_, unit> =
    choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]


let parseSingleQuotedString: Parser<_, unit> =
    between (pchar '\'') (pchar '\'') (manyChars (noneOf "'"))
