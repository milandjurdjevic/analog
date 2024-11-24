module Analog.Log

open System
open System.Collections.Generic
open System.IO

type Entry = Map<string, string>

module Pattern =
    open YamlDotNet.Serialization
    open YamlDotNet.Serialization.NamingConventions
    open GrokNet

    let extract (pattern: Grok) (text: string) : Entry list =
        pattern.Parse text
        |> Seq.map (fun i -> i.Key, i.Value.ToString())
        |> Seq.fold
            (fun list (key, value) ->
                match list with
                | [] -> [ Entry([ key, value ]) ]
                | head :: tail ->
                    if head.ContainsKey(key) then
                        [ Entry([ key, value ]); head ] @ tail
                    else
                        (head |> Map.add key value) :: tail)
            List.empty<Entry>
        |> List.rev
        
    let load path =
        DeserializerBuilder()
        |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
        |> _.Build()
        |> _.Deserialize<IDictionary<string, string>>(File.ReadAllText(path))
        |> Seq.map (fun pattern -> pattern.Key, Grok(pattern.Value))
        |> Map.ofSeq

module Filter =
    open FParsec

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

    let eval expression entry =
        let convert (obj: obj) : Literal =
            match obj with
            | :? string as x -> String x
            | :? int as x -> Integer x
            | :? float as x -> Float x
            | :? bool as x -> Boolean x
            | _ -> Null

        let rec eval' (expression: Expression) (entry: Entry) : obj =
            match expression with
            | Literal literal -> literal
            | Identifier identifier -> entry[identifier] |> convert :> obj
            | Binary(left, operator, right) ->
                let lEval = eval' left entry
                let rEval = eval' right entry

                match operator with
                | Equal -> lEval = rEval
                | NotEqual -> lEval <> rEval
                | GreaterThan -> (lEval :?> IComparable) > (rEval :?> IComparable)
                | GreaterThanOrEqual -> (lEval :?> IComparable) >= (rEval :?> IComparable)
                | LessThan -> (lEval :?> IComparable) < (rEval :?> IComparable)
                | LessThanOrEqual -> (lEval :?> IComparable) <= (rEval :?> IComparable)
                | And -> (lEval :?> bool) && (rEval :?> bool)
                | Or -> (lEval :?> bool) || (rEval :?> bool)

        eval' expression entry :?> bool

    let parse input =
        let quote: Parser<_, unit> = skipChar '\''

        let string: Parser<_, unit> =
            quote >>. manyCharsTill anyChar quote |>> String .>> spaces

        let int: Parser<_, unit> =
            numberLiteral NumberLiteralOptions.DefaultInteger "integer"
            |>> fun number -> int number.String
            |>> Integer
            .>> spaces

        let float: Parser<_, unit> =
            numberLiteral NumberLiteralOptions.DefaultFloat "float"
            |>> fun number -> float number.String
            |>> Float
            .>> spaces

        let boolean: Parser<_, unit> =
            choice [ pstringCI "true" >>% Boolean true; pstringCI "false" >>% Boolean false ]
            .>> spaces

        let none: Parser<_, unit> = pstringCI "null" >>% Null .>> spaces

        let literal: Parser<_, unit> =
            choice [ string; int; float; boolean; none ] |>> Literal

        let identifier: Parser<_, unit> =
            many1Chars (letter <|> digit) |>> Identifier .>> spaces

        let expression =
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

        match run expression input with
        | Success(result, _, _) -> Result.Ok result
        | Failure(errorMsg, _, _) -> Result.Error errorMsg

