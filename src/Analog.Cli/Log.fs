namespace Analog.Cli

open System
open System.Collections.Generic
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Text.RegularExpressions
open System.Threading
open Analog.Cli
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open FSharp.Control
open System.Linq

type Log =
    { Timestamp: DateTimeOffset
      Severity: string
      CustomDimensions: IReadOnlyDictionary<string, string> }

    static member Pattern =
        Regex(
            "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
            RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
        )

    static member private ofDimensions(dictionary: IReadOnlyDictionary<string, string>) =
        { Timestamp = dictionary["Timestamp"] |> DateTimeOffset.Parse
          Severity = dictionary["Severity"]
          CustomDimensions =
            dictionary
            |> Seq.filter (fun dimension -> dimension.Key <> "Timestamp" && dimension.Key <> "Severity")
            |> Seq.map (fun dimension -> dimension.Key, dimension.Value)
            |> readOnlyDict }

    static member private ofDimensions(groups: GroupCollection) =
        groups
        |> Seq.filter Value.hasType<Group>
        |> Seq.map (fun group -> group.Name, group.Value)
        |> readOnlyDict
        |> Log.ofDimensions

    static member ofStream
        ([<EnumeratorCancellation>] cancellationToken: CancellationToken)
        (pattern: Regex)
        (stream: Stream)
        =
        taskSeq {
            use reader = new StreamReader(stream, Encoding.UTF8)
            let batchSizeMax = 1000
            let mutable batchSize = batchSizeMax
            let mutable leftover = String.Empty

            while batchSize = batchSizeMax && not cancellationToken.IsCancellationRequested do
                let memory = Memory<char>(Array.zeroCreate batchSizeMax)
                let! blockSize = reader.ReadBlockAsync(memory, cancellationToken)
                batchSize <- blockSize

                let input = leftover + (memory.ToArray() |> Array.filter Value.notDefault |> String)
                let matches = pattern.Matches input |> Array.ofSeq

                for capture in matches.SkipLast 1 do
                    yield Log.ofDimensions capture.Groups

                leftover <-
                    match matches |> Seq.tryLast with
                    | None -> String.Empty
                    | Some value -> input[value.Index ..]

                if batchSize < batchSizeMax then
                    for capture in pattern.Matches leftover do
                        yield Log.ofDimensions capture.Groups
        }
