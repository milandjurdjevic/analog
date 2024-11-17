module Analog.Template

open System.IO
open System.Collections.Generic
open GrokNet
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

let load () =
    DeserializerBuilder()
    |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
    |> _.Build()
    |> _.Deserialize<IDictionary<string, string>>(File.ReadAllText("template.yml"))
    |> Seq.map (fun template -> template.Key, Grok(template.Value))
    |> Map.ofSeq

let parse (grok: Grok) (stream: Stream) =
    use reader = new StreamReader(stream)

    reader.ReadToEnd()
    |> grok.Parse
    |> Seq.map (fun i -> i.Key, i.Value)
    |> Seq.fold
        (fun list item ->
            match list with
            | [] -> [ Map<string, obj>([ item ]) ]
            | head :: tail ->
                if head.ContainsKey(fst item) then
                    [ Map<string, obj>([ item ]); head ] @ tail
                else
                    (head |> Map.add (fst item) (snd item)) :: tail)
        List.empty<Map<string, obj>>
    |> List.rev
