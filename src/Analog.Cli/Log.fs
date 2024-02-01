namespace Analog.Cli

open System
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

module Log =
    let private ofGroups (groups: GroupCollection) =
        groups
        |> Seq.filter (fun group -> group.GetType() = typeof<Group>)
        |> Seq.map (fun group -> group.Name, group.Value)
        |> Map.ofSeq

    let ofStream ([<EnumeratorCancellation>] cancellationToken: CancellationToken) (template: Regex) (stream: Stream) =
        taskSeq {
            use reader = new StreamReader(stream, Encoding.UTF8)
            let batchSizeMax = 1000
            let mutable batchSize = batchSizeMax
            let mutable leftover = String.Empty

            while batchSize = batchSizeMax && not cancellationToken.IsCancellationRequested do
                let memory = Memory<char>(Array.zeroCreate batchSizeMax)
                let! blockSize = reader.ReadBlockAsync(memory, cancellationToken)
                batchSize <- blockSize

                let input =
                    leftover + (memory.ToArray() |> Array.filter Unchecked.isNotDefault |> String)

                let matches = template.Matches input |> Array.ofSeq

                for capture in matches.SkipLast 1 do
                    yield ofGroups capture.Groups

                leftover <-
                    match matches |> Seq.tryLast with
                    | None -> String.Empty
                    | Some value -> input[value.Index ..]

                if batchSize < batchSizeMax then
                    for capture in template.Matches leftover do
                        yield ofGroups capture.Groups
        }
