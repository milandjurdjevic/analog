module Analog.Template

type Kind =
    | String
    | Integer
    | Float
    | Boolean
    | Timestamp

type Segment =
    | Field of string * Kind
    | Literal of string

module Parser =
    open FParsec

    let private kind: Parser<_, unit> =
        choice
            [ pstringCI "str" >>% Kind.String
              pstringCI "int" >>% Kind.Integer
              pstringCI "float" >>% Kind.Float
              pstringCI "bool" >>% Kind.Boolean
              pstringCI "ts" >>% Kind.Timestamp ]

    let private literal: Parser<_, unit> =
        many1CharsTill anyChar (lookAhead (skipChar '{') <|> eof) |>> Literal

    let private field: Parser<_, unit> =
        let name = many1Chars <| noneOf ":}"
        let opening = skipChar '{'
        let closing = skipChar '}'
        let between = skipChar ':'
        opening >>. name .>> between .>>. kind .>> closing |>> Field

    let private segment: Parser<_, unit> = choice [ field; literal ]

    let parse input =
        match run (many segment) input with
        | Success(result, _, _) -> Result.Ok result
        | Failure(error, _, _) -> Result.Error error
