module Analog.Extract

open GrokNet

type Result = Map<string, string>

let init txt =
    try
        Grok txt |> Result.Ok
    with err ->
        Result.Error err.Message

let def =
    "\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}"
    |> Grok

let private group list (key, value) =
    match list with
    | [] -> [ Map([ key, value ]) ]
    | head :: tail ->
        if head |> Map.containsKey key then
            [ Map([ key, value ]); head ] @ tail
        else
            (head |> Map.add key value) :: tail

let eval txt (grok: Grok) =
    grok.Parse txt
    |> Seq.map (fun i -> i.Key, i.Value.ToString())
    |> Seq.fold group List.empty<Map<string, string>>
    |> List.rev
