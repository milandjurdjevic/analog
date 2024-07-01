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

let toMap () =
    let text = File.ReadAllText("templates.yaml")

    DeserializerBuilder()
    |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
    |> _.Build()
    |> _.Deserialize<IDictionary<string, Template>>(text)
    |> Seq.map (fun template -> template.Key, template.Value)
    |> Map.ofSeq

type Iterable = { Regex: Regex; Stream: Stream }

let toIterable (stream: Stream) (template: Template) =
    { Regex = Regex(template.Regex, RegexOptions.Multiline)
      Stream = stream }

let iter (iterator: Map<string, string> -> unit) (iterable: Iterable) =
    // Read stream into the buffer, block by block. A block may contain a partial or a full match.
    //  - Partial match happens when there is only one regex match in the buffer and the buffer contains
    //    only assigned characters. This means that some of the log characters are still not read from the stream, so
    //    we need to keep track of all previous buffers that are related to the current log entry. Buffered characters
    //    are being tracked until you find a complete match. After that you construct a string and reset buffer.
    //  - Full match happens there are several matches in the buffer. We can treat the last match
    //    as a partial, unless the buffer ends with unassigned characters, and all previous as full matches. There
    //    is a possibility that the buffer ends with unmatched characters, so they must be passed to the next buffer.
    // Full matches are iterable, while partials must be constructed to a full match first.
    use reader = new StreamReader(iterable.Stream, Encoding.UTF8)
    let regexTuple (regexGroup: Group) = regexGroup.Name, regexGroup.Value
    let regexTuples regexGroups = regexGroups |> Seq.map regexTuple
    let regexGroups (regexMatch: Match) = regexMatch.Groups |> Seq.skip 1

    let regexMap regexMatch =
        regexMatch |> regexGroups |> regexTuples |> Map.ofSeq

    let isAssignedChar char = char <> Unchecked.defaultof<char>
    let assignedChars chars = chars |> Array.filter isAssignedChar

    let partiallyMatched (text: string) (regexMatches: MatchCollection) =
        match regexMatches |> Seq.tryLast with
        | Some value -> text[value.Index ..]
        | None -> text

    let rec nextBlock (leftover: string) =
        let buffer = Array.zeroCreate<char> 1024
        let block = reader.ReadBlock(buffer, 0, buffer.Length)
        let text = buffer |> assignedChars |> string |> (fun current -> leftover + current)
        let matches = iterable.Regex.Matches text
        matches.SkipLast 1 |> Seq.map regexMap |> Seq.iter iterator
        let partial = partiallyMatched text matches

        if block < buffer.Length then
            iterable.Regex.Matches partial |> Seq.map regexMap |> Seq.iter iterator
        else
            nextBlock partial

    nextBlock String.Empty
