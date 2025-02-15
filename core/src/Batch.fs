module Analog.Core.Batch

open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics.CodeAnalysis

type Props = { Delimiter: Regex; Buffer: int }

let setup ([<StringSyntax("regex")>] start) buffer =
    { Delimiter = Regex(start, RegexOptions.Multiline ||| RegexOptions.Compiled, TimeSpan.FromSeconds(int64 5))
      Buffer = buffer }

let load (stream: Stream) props : string seq =
    seq {
        use reader = new StreamReader(stream)
        let mutable leftover = String.Empty

        while not reader.EndOfStream do
            // Read a chunk of data from the stream.
            let buffer = Array.zeroCreate<char> props.Buffer
            let block = reader.ReadBlock(buffer)
            let content = leftover + (buffer |> Array.take block |> String.Concat)
            let matches = props.Delimiter.Matches(content)

            // Collect the matches found in the chunk and the last match index.
            let entries, index =
                ((List.empty<string>, 0), matches)
                ||> Seq.fold (fun (list, index) item ->
                    let entry = content[index .. item.Index - 1].Trim()
                    (if entry.Length > 0 then list @ [ entry ] else list), item.Index)

            // Store any leftover content for the next iteration.
            leftover <- if entries.Length > 0 then content[index..] else content

            // Yield each entry found in the current chunk, separately.
            yield! entries

        let last = leftover.Trim()
        // Ensure the last entry is not lost.
        if last.Length > 0 then
            yield last
    }