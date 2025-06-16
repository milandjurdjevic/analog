namespace Analog

open System

module FParsec =
    open FParsec

    let parseDateTimeOffset: Parser<_, unit> =
        restOfLine false
        >>= fun input ->
            try
                DateTimeOffset.Parse input |> preturn
            with err ->
                fail err.Message
        |> attempt

    let parseFiniteFloat: Parser<_, unit> =
        pfloat
        >>= fun res ->
            if Double.IsInfinity res || Double.IsNaN res then
                fail "Number cannot be infinite or NaN"
            else
                preturn res
        |> attempt

    let parseCIBoolean: Parser<_, unit> =
        choice [ pstringCI "true" >>% true; pstringCI "false" >>% false ]
