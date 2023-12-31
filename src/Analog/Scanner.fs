namespace Analog

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.RegularExpressions
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Control
open Microsoft.FSharp.Core

module Scanner =
    let private notDefault value = value <> Unchecked.defaultof<'a>

    let private exactlyOfType<'a> enumerable =
        enumerable |> Seq.filter (fun a -> a.GetType() = typeof<'a>)

    let private pattern =
        Regex(
            "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
            RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
        )


    let Scan (stream: Stream) =
        async {
            use reader = new StreamReader(stream, Encoding.UTF8)

            let rec scan (leftover: char array) (data: IReadOnlyDictionary<string, string> seq) =
                async {
                    let buffer = Array.zeroCreate 1000
                    let! count = reader.ReadBlockAsync(buffer, 0, buffer.Length) |> Async.AwaitTask
                    let text = buffer |> Array.takeWhile notDefault |> Array.append leftover |> String

                    let group (capture: Match) =
                        capture.Groups
                        |> exactlyOfType<Group>
                        |> Seq.map (fun group -> group.Name, group.Value)
                        |> readOnlyDict

                    if count = 0 && leftover.Length = 0 then
                        // There is nothing more to be processed, so just return the accumulated result.
                        return data
                    elif count = 0 && leftover.Length > 0 then
                        // Nothing is read from the stream, but there is still some characters left to be processed.
                        return pattern.Matches text |> Seq.map group |> Seq.append data
                    else
                        // Analyze input and pass the combined result to another iteration.
                        let matches = pattern.Matches text |> Array.ofSeq

                        let leftover =
                            match matches |> Array.tryLast with
                            | None -> Array.empty
                            | Some value -> text[value.Index ..].ToCharArray()

                        let data = matches[.. matches.Length - 2] |> Seq.map group |> Seq.append data

                        return! scan leftover data
                }

            return! scan Array.empty Array.empty
        }
        |> Async.StartAsTask
