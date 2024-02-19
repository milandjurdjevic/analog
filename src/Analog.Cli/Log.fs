module Analog.Cli.Log

open System
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Text.RegularExpressions
open System.Threading
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open FSharp.Control
open System.Linq

let private parseDimensionValue (map: Map<string, string>) (dimension: Dimension) =
    map.TryFind dimension.Name
    |> Option.map dimension.Parse
    |> Option.flatten
    |> Option.defaultValue String.Empty

let private mapDimensions (dimensions: Dimension list) (map: Map<string, string>) =
    dimensions
    |> List.map (fun dimension -> dimension.Name, dimension |> parseDimensionValue map)
    |> readOnlyDict

let private mapRegexMatch (regexMatch: Match) =
    regexMatch.Groups
    |> Seq.filter (fun object -> object.GetType() = typeof<Group>)
    |> Seq.map (fun group -> group.Name, group.Value)
    |> Map.ofSeq

let private charMemoryToString (memory: Memory<char>) =
    memory.ToArray()
    |> Array.filter (fun char -> char <> Unchecked.defaultof<char>)
    |> String

let ofStream (template: Template) ([<EnumeratorCancellation>] token: CancellationToken) (stream: Stream) =
    taskSeq {
        use reader = new StreamReader(stream, Encoding.UTF8)
        let batchSizeMax = 1000
        let mutable batchSize = batchSizeMax
        let mutable leftover = String.Empty

        let regex =
            Regex(template.Regex, RegexOptions.Compiled ||| RegexOptions.Multiline ||| RegexOptions.IgnoreCase)

        while batchSize = batchSizeMax && not token.IsCancellationRequested do
            let memory = Memory<char>(Array.zeroCreate batchSizeMax)
            let! blockSize = reader.ReadBlockAsync(memory, token)
            batchSize <- blockSize

            let input = leftover + charMemoryToString memory
            let matches = regex.Matches input |> Array.ofSeq

            for regexMatch in matches.SkipLast 1 do
                yield regexMatch |> mapRegexMatch |> mapDimensions template.Dimensions

            leftover <-
                match matches |> Seq.tryLast with
                | None -> String.Empty
                | Some value -> input[value.Index ..]

            if batchSize < batchSizeMax then
                for regexMatch in regex.Matches leftover do
                    yield regexMatch |> mapRegexMatch |> mapDimensions template.Dimensions
    }
