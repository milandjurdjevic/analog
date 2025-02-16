module Analog.Core.Grok

open GrokNet

type Pattern = Pattern of Grok
type Extract = Extract of GrokResult
type Group = Group of Map<string, string>

let pattern: Pattern =
    Grok("\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}")
    |> Pattern

let create (pattern: string) : Result<Pattern, string> =
    try
        Grok(pattern) |> Pattern |> Result.Ok
    with err ->
        $"Grok initialization failed with error: {err.Message}" |> Result.Error

let extract (text: string) (Pattern pattern) : Result<Extract, string> =
    try
        pattern.Parse text |> Extract |> Result.Ok
    with err ->
        $"Grok extraction failed with error: {err.Message}" |> Result.Error

let group (Extract extract) =
    extract
    |> Seq.fold
        (fun list item ->
            match list with
            | [] -> [ Map([ item.Key, string item.Value ]) ]
            | head :: tail ->
                if head |> Map.containsKey item.Key then
                    [ Map([ item.Key, string item.Value ]); head ] @ tail
                else
                    (head |> Map.add item.Key (string item.Value)) :: tail)
        List.empty
    |> List.rev
    |> List.map Group

let transform: Group list -> Log.Entry list =
    List.map (fun (Group group) ->
        group
        |> Map.toSeq
        |> Seq.choose (fun (key, value) ->
            match Parser.logLiteral |> Parser.parse value with
            | Ok value -> Some(key, value)
            | Error _ -> None)
        |> Map.ofSeq
        |> Log.Entry)
