module Analog.Import

open System
open System.Linq
open System.IO
open System.Text.RegularExpressions

/// <summary>
///     Parses the given stream using the provided regular expression and applies a function to each match.
///     This function reads the stream in blocks, matches each block against the regex,
///     and applies the function to each match. It handles partial matches that span across block boundaries.
/// </summary>
/// <param name="tap">
///     A function to apply to each map of named groups found in matches.
///     The map represents the named groups captured by the regex in a single match.
/// </param>
/// <param name="pattern">The regular expression used to find matches in the stream.</param>
/// <param name="stream">The stream to be parsed.</param>
let parse (tap: Map<string, string> -> unit) (pattern: string) (stream: Stream) =
    use reader = new StreamReader(stream)

    let regex =
        Regex(pattern, RegexOptions.Multiline ||| RegexOptions.Compiled, TimeSpan.FromSeconds(5))

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
        matches.SkipLast 1 |> Seq.map regexMap |> Seq.iter tap

        if block < buffer.Length then
            regex.Matches(regexLeftover input matches) |> Seq.map regexMap |> Seq.iter tap
        else
            nextBlock (regexLeftover input matches)

    nextBlock String.Empty
