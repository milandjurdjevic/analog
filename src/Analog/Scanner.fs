namespace Analog

open System.IO
open System.Text.RegularExpressions
open System.Threading
open Microsoft.FSharp.Control
open Microsoft.FSharp.Core

module Scanner =

    let private pattern =
        Regex(
            "^(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})? (?<Severity>\[[A-Z]{3}\]) (?<Message>[\s\S]*?\n*(?=^[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
            RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
        )

    let private group (capture: Match) =
        capture.Groups
        |> Seq.filter (fun group -> group.GetType() = typeof<Group>)
        |> Seq.map (fun group -> group.Name, group.Value)
        |> readOnlyDict

    let Scan (stream: Stream, cancellationToken: CancellationToken) =
        task {
            use reader = new StreamReader(stream)
            let! content = reader.ReadToEndAsync(cancellationToken)
            return pattern.Matches content |> Seq.map group
        }
