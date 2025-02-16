module Analog.Core.Batch

open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics.CodeAnalysis
open FSharp.Control

type Capture = string -> string seq * string

let capture ([<StringSyntax("regex")>] regex) : Capture =

    let regex =
        Regex(regex, RegexOptions.Multiline ||| RegexOptions.Compiled, TimeSpan.FromSeconds(int64 5))

    fun input ->
        let entries, index =
            ((List.empty<string>, 0), regex.Matches input)
            ||> Seq.fold (fun (list, index) item ->
                let entry = input[index .. item.Index - 1].Trim()
                (if entry.Length > 0 then list @ [ entry ] else list), item.Index)

        match entries with
        | [] -> [], input
        | entries -> entries, input[index..]

let sync (stream: Stream) (capture: Capture) : string seq =
    seq {
        use reader = new StreamReader(stream)
        let mutable leftover = String.Empty

        while not reader.EndOfStream do
            let buffer = Array.zeroCreate<char> 1024
            let block = reader.ReadBlock buffer
            let cap = leftover + (buffer |> Array.take block |> String.Concat) |> capture
            leftover <- snd cap
            yield! fst cap

        yield!
            match leftover.Trim() with
            | "" -> Seq.empty
            | leftover -> Seq.singleton leftover
    }

let async (stream: Stream) (capture: Capture) =
    taskSeq {
        use reader = new StreamReader(stream)
        let mutable leftover = String.Empty

        while not reader.EndOfStream do
            let buffer = Array.zeroCreate<char> 1024
            let! block = reader.ReadBlockAsync(Memory(buffer))
            let cap = leftover + (buffer |> Array.take block |> String.Concat) |> capture
            leftover <- snd cap
            yield! fst cap

        yield!
            match leftover.Trim() with
            | "" -> Seq.empty
            | leftover -> Seq.singleton leftover
    }
