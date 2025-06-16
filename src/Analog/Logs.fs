namespace Analog

open System

type LogPattern = private LogPattern of GrokNet.Grok

[<RequireQualifiedAccess>]
type LogLiteral =
    | String of string
    | Number of float
    | Boolean of bool
    | Timestamp of DateTimeOffset

type LogEntry = private LogEntry of Map<string, LogLiteral>

[<RequireQualifiedAccess>]
module LogLiteral =
    open FParsec

    let private parse: Parser<_, unit> =
        choice
            [ parseDateTimeOffset |>> LogLiteral.Timestamp
              parseFiniteFloat |>> LogLiteral.Number
              parseCIBoolean |>> LogLiteral.Boolean
              restOfLine true |>> LogLiteral.String ]

    let tryCreate literal =
        match run parse literal with
        | Success(result, _, _) -> Some result
        | Failure _ -> None

    let box =
        function
        | LogLiteral.Boolean bool -> box bool
        | LogLiteral.String str -> box str
        | LogLiteral.Number num -> box num
        | LogLiteral.Timestamp ts -> box ts

[<RequireQualifiedAccess>]
module LogEntry =
    let create = LogEntry
    let value (LogEntry entry) = entry

    let box =
        value >> Map.map (fun _ -> LogLiteral.box) >> Map.toSeq >> readOnlyDict >> box

[<RequireQualifiedAccess>]
module LogPattern =
    open GrokNet

    let value (LogPattern pattern) = pattern

    let create = Grok >> LogPattern

    let standard =
        create "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:level}\] %{GREEDYDATA:message}"

    let parse text =
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
            >> Seq.choose (fun (key, value) -> LogLiteral.tryCreate value |> Option.map (fun literal -> key, literal))
            >> Map.ofSeq
            >> LogEntry.create
        )
