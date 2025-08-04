module Analog.Logs

open System
open GrokNet
open FParsec

type Literal =
    | StringLiteral of string
    | NumberLiteral of float
    | BooleanLiteral of bool
    | TimestampLiteral of DateTimeOffset

type Entry = private Entry of Map<string, Literal>

type Pattern = private Pattern of Grok

[<RequireQualifiedAccess>]
module Literal =
    let parse =
        choice
            [ Timestamp.parse |>> TimestampLiteral
              Number.parse |>> NumberLiteral
              Boolean.parse |>> BooleanLiteral
              restOfLine true |>> StringLiteral ]

    let create literal =
        match run parse literal with
        | Success(result, _, _) -> Some result
        | Failure _ -> None

    let toObj =
        function
        | BooleanLiteral bool -> box bool
        | StringLiteral str -> box str
        | NumberLiteral num -> box num
        | TimestampLiteral ts -> box ts

[<RequireQualifiedAccess>]
module Entry =
    let create = Entry
    let value (Entry entry) = entry

    let toObj =
        value >> Map.map (fun _ -> Literal.toObj) >> Map.toSeq >> readOnlyDict >> box

[<RequireQualifiedAccess>]
module Pattern =
    let value (Pattern pattern) = pattern
    let create = Grok >> Pattern

    let preset =
        create "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:level}\] %{GREEDYDATA:message}"

    let eval text =
        value
        >> _.Parse(text)
        >> Seq.fold
            (fun list item ->
                match list with
                | [] -> [ Map([ item.Key, string item.Value ]) ]
                | head :: tail ->
                    if head |> Map.containsKey item.Key then
                        [ Map([ item.Key, string item.Value ]); head ] @ tail
                    else
                        (head |> Map.add item.Key (string item.Value)) :: tail)
            List.empty
        >> List.rev
        >> List.map (
            Map.toSeq
            >> Seq.choose (fun (key, value) -> Literal.create value |> Option.map (fun literal -> key, literal))
            >> Map.ofSeq
            >> Entry.create
        )
