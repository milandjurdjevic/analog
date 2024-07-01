module Analog.Templates

open System
open System.IO
open System.Linq
open System.Text
open System.Text.RegularExpressions
open System.Collections.Generic
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

type Template() =
    member val Regex: string = String.Empty with get, set

type Iterable = { Regex: Regex; Stream: Stream }

let templateMap =
    DeserializerBuilder()
    |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
    |> _.Build()
    |> _.Deserialize<IDictionary<string, Template>>(File.ReadAllText("templates.yml"))
    |> Seq.map (fun template -> template.Key, template.Value)
    |> Map.ofSeq

let toIterable (stream: Stream) (template: Template) =
    { Regex = Regex(template.Regex, RegexOptions.Multiline)
      Stream = stream }

let iter (iterator: Map<string, string> -> unit) (iterable: Iterable) =
    use reader = new StreamReader(iterable.Stream, Encoding.UTF8)
    let regexTuple (regexGroup: Group) = regexGroup.Name, regexGroup.Value
    let regexTuples regexGroups = regexGroups |> Seq.map regexTuple
    let regexGroups (regexMatch: Match) = regexMatch.Groups |> Seq.skip 1

    let regexMap regexMatch =
        regexMatch |> regexGroups |> regexTuples |> Map.ofSeq

    let regexLeftover (regexInput: string) (regexMatches: Match seq) =
        match regexMatches |> Seq.tryLast with
        | Some value -> regexInput[value.Index ..]
        | None -> regexInput

    let assignedChar char = char <> Unchecked.defaultof<char>
    let assignedChars chars = chars |> Array.filter assignedChar

    let rec nextBlock (leftover: string) =
        let buffer = Array.zeroCreate<char> 1024
        let block = reader.ReadBlock(buffer, 0, buffer.Length)
        let input = buffer |> assignedChars |> String |> (fun current -> leftover + current)
        let matches = iterable.Regex.Matches input
        matches.SkipLast 1 |> Seq.map regexMap |> Seq.iter iterator

        if block < buffer.Length then
            iterable.Regex.Matches(regexLeftover input matches)
            |> Seq.map regexMap
            |> Seq.iter iterator
        else
            nextBlock (regexLeftover input matches)

    nextBlock String.Empty
