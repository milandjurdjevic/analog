module Analog.Core.Batch

open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics.CodeAnalysis

let load ([<StringSyntax("regex")>] start) chunk (stream: Stream) : string seq =
    seq {
        use reader = new StreamReader(stream)
        let mutable leftover = String.Empty

        let regex =
            Regex(start, RegexOptions.Multiline ||| RegexOptions.Compiled, TimeSpan.FromSeconds(int64 5))

        while not reader.EndOfStream do
            let buffer = Array.zeroCreate<char> chunk
            let block = reader.ReadBlock(buffer)
            let content = leftover + (buffer |> Array.take block |> String.Concat)
            let matches = regex.Matches(content)

            let entries, index =
                ((List.empty<string>, 0), matches)
                ||> Seq.fold (fun (list, index) item ->
                    let entry = content[index .. item.Index - 1].Trim()
                    (if entry.Length > 0 then entry :: list else list), item.Index)

            yield! List.rev entries

            if entries.Length > 0 then
                leftover <- content[index..]
            else
                leftover <- content


        let last = leftover.Trim()
        // Ensure the last entry is not lost.
        if last.Length > 0 then
            yield last
    }
