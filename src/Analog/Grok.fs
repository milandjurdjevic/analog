module Analog.Grok

open GrokNet

type Entry =
    | Raw of Map<string, string>
    | Mold of Map<string, obj>

let grok (pattern: Grok) (text: string) : Entry list =
    pattern.Parse text
    |> Seq.map (fun i -> i.Key, i.Value.ToString())
    |> Seq.fold
        (fun list (key, value) ->
            match list with
            | [] -> [ Map([ key, value ]) ]
            | head :: tail ->
                if head.ContainsKey(key) then
                    [ Map([ key, value ]); head ] @ tail
                else
                    (head |> Map.add key value) :: tail)
        List.empty<Map<string, string>>
    |> List.rev
    |> List.map Raw