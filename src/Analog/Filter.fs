module Analog.Filter

open System

type Operator =
    | Equal
    | NotEqual
    | GreaterThan
    | GreaterThanOrEqual
    | LessThan
    | LessThanOrEqual
    | And
    | Or

type Literal =
    | String of string
    | Integer of int
    | Float of float
    | Boolean of bool
    | Null

type Expression =
    | Literal of Literal
    | Identifier of string
    | Binary of Expression * Operator * Expression

module Evaluator =
    let rec private eval' (expression: Expression) (entry: Map<string, string>) =
        match expression with
        | Literal literal -> literal :> obj
        | Identifier identifier -> entry[identifier] :> obj
        | Binary(left, operator, right) ->
            let lEval = eval' left entry
            let rEval = eval' right entry
            // TODO: lEval and rEval must have the same type - cast the identifier to the literal type.
            match operator with
            | Equal -> lEval = rEval
            | NotEqual -> lEval <> rEval
            | GreaterThan -> (lEval :?> IComparable) > (rEval :?> IComparable)
            | GreaterThanOrEqual -> (lEval :?> IComparable) >= (rEval :?> IComparable)
            | LessThan -> (lEval :?> IComparable) < (rEval :?> IComparable)
            | LessThanOrEqual -> (lEval :?> IComparable) <= (rEval :?> IComparable)
            | And -> (lEval :?> bool) && (rEval :?> bool)
            | Or -> (lEval :?> bool) || (rEval :?> bool)

    let eval expression entry = eval' expression entry :?> bool

module Parser =
    open FParsec

    let private quote: Parser<_, unit> = skipChar '\''

    let private stringLiteral: Parser<_, unit> =
        quote >>. manyCharsTill anyChar quote |>> String .>> spaces

    let private integerLiteral: Parser<_, unit> =
        numberLiteral NumberLiteralOptions.DefaultInteger "integer"
        |>> fun number -> int number.String
        |>> Integer
        .>> spaces

    let private floatLiteral: Parser<_, unit> =
        numberLiteral NumberLiteralOptions.DefaultFloat "float"
        |>> fun number -> float number.String
        |>> Float
        .>> spaces

    let private booleanLiteral: Parser<_, unit> =
        choice [ pstringCI "true" >>% Boolean true; pstringCI "false" >>% Boolean false ]
        .>> spaces

    let private nullLiteral: Parser<_, unit> = pstringCI "null" >>% Null .>> spaces

    let private literal: Parser<_, unit> =
        choice [ stringLiteral; integerLiteral; floatLiteral; booleanLiteral; nullLiteral ]
        |>> Literal

    let private identifier: Parser<_, unit> =
        many1Chars (letter <|> digit) |>> Identifier .>> spaces

    let private expression =
        let precedence = OperatorPrecedenceParser<Expression, _, _>()
        precedence.TermParser <- choice [ literal; identifier ]
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

    let parse input =
        match run expression input with
        | Success(result, _, _) -> Result.Ok result
        | Failure(errorMsg, _, _) -> Result.Error errorMsg
