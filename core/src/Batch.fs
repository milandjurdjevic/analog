module Analog.Core.Batch

open System
open System.IO
open System.Text.RegularExpressions
open System.Diagnostics.CodeAnalysis

/// <summary>Loads and processes a stream of text data in chunks, using a regular expression to delimit entries.</summary>
/// <param name="start">A string representing the starting pattern for the regular expression.</param>
/// <param name="chunk">The size of each chunk to read from the stream.</param>
/// <param name="stream">The input stream to read from.</param>
/// <returns>A sequence of strings, each representing a matched entry.</returns>
let load ([<StringSyntax("regex")>] start) chunk (stream: Stream) : string seq =
    seq {
        use reader = new StreamReader(stream)
        let mutable leftover = String.Empty

        // Compile the regular expression with multiline and compiled options.
        let regex =
            Regex(start, RegexOptions.Multiline ||| RegexOptions.Compiled, TimeSpan.FromSeconds(int64 5))

        while not reader.EndOfStream do
            // Read a chunk of data from the stream.
            let buffer = Array.zeroCreate<char> chunk
            let block = reader.ReadBlock(buffer)
            let content = leftover + (buffer |> Array.take block |> String.Concat)
            let matches = regex.Matches(content)

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
