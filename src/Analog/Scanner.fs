module Analog.Scanner

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Threading
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open FSharp.Control
open System.Linq

let private notDefault value = value <> Unchecked.defaultof<'a>

let private exactlyOfType<'a> enumerable =
    enumerable |> Seq.filter (fun a -> a.GetType() = typeof<'a>)

let private toDictionary (capture: Match) =
    capture.Groups.Values
    |> exactlyOfType<Group>
    |> Seq.map (fun g -> g.Name, g.Value)
    |> readOnlyDict

let private matchLeftover (input: string) (matches: Match array) =
    match matches |> Seq.tryLast with
    | None -> String.Empty
    | Some value -> input[value.Index ..]

let private pattern =
    Regex(
        "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
        RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
    )

let Scan (stream: Stream) (cancellationToken: CancellationToken) =
    taskSeq {
        use reader = new StreamReader(stream, Encoding.UTF8)
        let batchSizeMax = 1000
        let mutable batchSize = batchSizeMax
        let mutable leftover = String.Empty

        while batchSize = batchSizeMax do
            let memory = Memory<char>(Array.zeroCreate batchSizeMax)
            let! blockSize = reader.ReadBlockAsync(memory, cancellationToken)
            batchSize <- blockSize
            let input = leftover + (memory.ToArray() |> Array.filter notDefault |> String)
            let matches = pattern.Matches input |> Array.ofSeq

            for capture in matches.SkipLast 1 do
                yield toDictionary capture

            leftover <- matchLeftover input matches

            if batchSize < batchSizeMax then
                for capture in pattern.Matches leftover do
                    yield toDictionary capture
    }
