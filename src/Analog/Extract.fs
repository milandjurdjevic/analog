module Analog.Extract

open System
open System.Linq
open System.IO
open System.Text.RegularExpressions

type Handler = Map<string, string> -> unit

let stream (handler: Handler) (template: string) (stream: Stream) =
    use reader = new StreamReader(stream)
    let regexOptions = RegexOptions.Multiline ||| RegexOptions.Compiled
    let regex = Regex(template, regexOptions, TimeSpan.FromSeconds(5))
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
        let matches = regex.Matches input
        matches.SkipLast 1 |> Seq.map regexMap |> Seq.iter handler

        if block < buffer.Length then
            regex.Matches(regexLeftover input matches)
            |> Seq.map regexMap
            |> Seq.iter handler
        else
            nextBlock (regexLeftover input matches)

    nextBlock String.Empty
