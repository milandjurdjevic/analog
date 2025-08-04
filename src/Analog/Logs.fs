module Analog.Logs

open System
open FParsec
open GrokNet

[<AutoOpen>]
module Literals =
    type Literal =
        | StringLiteral of string
        | NumberLiteral of float
        | BooleanLiteral of bool
        | TimestampLiteral of DateTimeOffset

    let parseLiteral =
        choice
            [ parseDateTimesOffset |>> TimestampLiteral
              parseNonInfiniteDouble |>> NumberLiteral
              parseBoolean |>> BooleanLiteral
              restOfLine true |>> StringLiteral ]

    let createLiteral literal =
        match run parseLiteral literal with
        | Success(result, _, _) -> Some result
        | Failure _ -> None

    let boxLiteral =
        function
        | BooleanLiteral bool -> box bool
        | StringLiteral str -> box str
        | NumberLiteral num -> box num
        | TimestampLiteral ts -> box ts

[<AutoOpen>]
module Entries =
    type Entry = private Entry of Map<string, Literal>

    let createEntry = Entry
    let getEntry (Entry entry) = entry

    let boxEntry =
        getEntry >> Map.map (fun _ -> boxLiteral) >> Map.toSeq >> readOnlyDict >> box

[<AutoOpen>]
module Patterns =
    type Pattern = private Pattern of Grok

    let getPattern (Pattern pattern) = pattern
    let createPattern = Grok >> Pattern

    let defaultPattern =
        createPattern "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:level}\] %{GREEDYDATA:message}"

    let evalPattern text =
        getPattern
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
            >> Seq.choose (fun (key, value) -> createLiteral value |> Option.map (fun literal -> key, literal))
            >> Map.ofSeq
            >> createEntry
        )
