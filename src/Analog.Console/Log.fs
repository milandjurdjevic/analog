module Analog.Console.Log

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

let private pattern =
    Regex(
        "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
        RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
    )

let ofStream ([<EnumeratorCancellation>] cancellationToken: CancellationToken) (stream: Stream) =
    taskSeq {
        use reader = new StreamReader(stream, Encoding.UTF8)
        let batchSizeMax = 1000
        let mutable batchSize = batchSizeMax
        let mutable leftover = String.Empty

        let groupToDictionary (groups: GroupCollection) =
            groups
            |> Seq.filter Value.hasType<Group>
            |> Seq.map (fun group -> group.Name, group.Value)
            |> readOnlyDict

        while batchSize = batchSizeMax && not cancellationToken.IsCancellationRequested do
            let memory = Memory<char>(Array.zeroCreate batchSizeMax)
            let! blockSize = reader.ReadBlockAsync(memory, cancellationToken)
            batchSize <- blockSize

            let input = leftover + (memory.ToArray() |> Array.filter Value.notDefault |> String)
            let matches = pattern.Matches input |> Array.ofSeq

            for capture in matches.SkipLast 1 do
                yield groupToDictionary capture.Groups

            leftover <-
                match matches |> Seq.tryLast with
                | None -> String.Empty
                | Some value -> input[value.Index ..]

            if batchSize < batchSizeMax then
                for capture in pattern.Matches leftover do
                    yield groupToDictionary capture.Groups
    }
