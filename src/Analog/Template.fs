module Analog.Template

type Kind =
    | String
    | Integer
    | Float
    | Boolean
    | Timestamp

type Segment =
    | Field of string * Kind

module Parser =
    open FParsec

    let private kind: Parser<_, unit> =
        choice
            [ pstringCI "string" >>% Kind.String
              pstringCI "integer" >>% Kind.Integer
              pstringCI "float" >>% Kind.Float
              pstringCI "boolean" >>% Kind.Boolean
              pstringCI "timestamp" >>% Kind.Timestamp ]

    let private field: Parser<_, unit> =
        let name = many1Chars <| noneOf ":}"
        let opening = skipChar '{'
        let closing = skipChar '}'
        let between = skipChar ':'
        opening >>. name .>> between .>>. kind .>> closing |>> Field

    let parse input =
        match run field input with
        | Success(result, _, _) -> Result.Ok result
        | Failure(error, _, _) -> Result.Error error
