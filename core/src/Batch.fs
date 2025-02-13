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
            let text = leftover + (buffer |> Array.take block |> String.Concat)

            let matches = regex.Matches(text)

            if matches.Count = 0 then
                leftover <- text
            else
                let mutable index = 0

                for m in matches do
                    let entry = text.Substring(index, m.Index - index)

                    let trimmedEntry = entry.Trim()
                    if trimmedEntry <> "" then
                        yield trimmedEntry

                    index <- m.Index

                leftover <- text.Substring(index)

        let trimmedLeftover = leftover.Trim()
        if trimmedLeftover <> "" then
            yield trimmedLeftover // Ensure the last entry is not lost.
    }
