namespace Analog.Cli

open System
open System.Collections.Generic
open System.IO
open System.Runtime.CompilerServices
open System.Text
open System.Text.RegularExpressions
open System.Threading
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open FSharp.Control
open System.Linq
open System.Linq.Dynamic.Core

module Log =

    let private ofRegexMatch (regexMatch: Match) =
        regexMatch.Groups
        |> Seq.filter (fun object -> object.GetType() = typeof<Group>)
        |> Seq.map (fun group -> group.Name, group.Value)
        |> readOnlyDict

    let ofStream (regex: Regex) ([<EnumeratorCancellation>] token: CancellationToken) (stream: Stream) =
        taskSeq {
            use reader = new StreamReader(stream, Encoding.UTF8)
            let batchSizeMax = 1000
            let mutable batchSize = batchSizeMax
            let mutable leftover = String.Empty

            while batchSize = batchSizeMax && not token.IsCancellationRequested do
                let memory = Memory<char>(Array.zeroCreate batchSizeMax)
                let! blockSize = reader.ReadBlockAsync(memory, token)
                batchSize <- blockSize

                let input =
                    leftover
                    + (memory.ToArray()
                       |> Array.filter (fun char -> char <> Unchecked.defaultof<char>)
                       |> String)

                let matches = regex.Matches input |> Array.ofSeq

                for regexMatch in matches.SkipLast 1 do
                    yield ofRegexMatch regexMatch

                leftover <-
                    match matches |> Seq.tryLast with
                    | None -> String.Empty
                    | Some value -> input[value.Index ..]

                if batchSize < batchSizeMax then
                    for regexMatch in regex.Matches leftover do
                        yield ofRegexMatch regexMatch
        }

    let ofFiles (regex: Regex) ([<EnumeratorCancellation>] token: CancellationToken) (paths: string seq) =
        taskSeq {
            for path in paths |> Seq.filter File.Exists do
                use stream = File.OpenRead path
                yield! stream |> ofStream regex token
        }

    let filter (expression: string) (logs: IReadOnlyDictionary<string, string> list) =
        if String.IsNullOrWhiteSpace expression then
            logs
        else
            logs.AsQueryable().Where(expression) |> Seq.toList
