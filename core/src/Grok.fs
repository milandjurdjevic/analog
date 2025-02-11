[<RequireQualifiedAccess>]
module Analog.Core.Grok

open GrokNet

type private LogEntry = Log.Entry

type Group = Group of Map<string, string>

let create (pattern: string) : Result<Grok, string> =
    try
        Grok(pattern) |> Result.Ok
    with err ->
        $"Grok initialization failed with error: {err.Message}" |> Result.Error

let extract (text: string) (pattern: Grok) : Result<GrokResult, string> =
    try
        pattern.Parse text |> Result.Ok
    with err ->
        $"Grok extraction failed with error: {err.Message}" |> Result.Error

let pattern: Grok =
    Grok("\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}")

let group: GrokResult -> Group list =
    Seq.fold
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
    >> List.map Group

let transform: Group list -> LogEntry list =
    List.map (fun (Group group) ->
        group
        |> Map.toSeq
        |> Seq.choose (fun (key, value) ->
            match Parser.literal |> Parser.parse value with
            | Ok value -> Some(key, value)
            | Error _ -> None)
        |> Map.ofSeq
        |> LogEntry.Entry)
