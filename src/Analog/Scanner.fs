namespace Analog

open System
open System.IO
open System.Text.RegularExpressions
open System.Threading
open Microsoft.FSharp.Control
open Microsoft.FSharp.Core

module Scanner =

    type private String with

        member this.ToEnum<'a when 'a :> Enum and 'a: struct and 'a: (new: unit -> 'a)>() =
            let ok, value = Enum.TryParse<'a>(this, true)
            if ok then Some value else None

        member this.ToDateTimeOffset() =
            let ok, value = DateTimeOffset.TryParse(this)
            if ok then Some value else None

    let private pattern =
        Regex(
            "^(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})? (?<Severity>\[[A-Z]{3}\]) (?<Message>[\s\S]*?\n*(?=^[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
            RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
        )

    type Log =
        { Severity: String
          Timestamp: DateTimeOffset
          Message: String }

    let private parse (matching: Match) : Log =
        { Timestamp =
            match (matching.Groups.Values |> Seq.tryFind (fun v -> v.Name = "Timestamp")) with
            | None -> DateTimeOffset.MinValue
            | Some value ->
                match value.Value.ToDateTimeOffset() with
                | None -> DateTimeOffset.MinValue
                | Some value -> value
          Severity =
            match (matching.Groups.Values |> Seq.tryFind (fun v -> v.Name = "Severity")) with
            | None -> String.Empty
            | Some value -> value.Value.Trim('[', ']')
          Message =
            match (matching.Groups.Values |> Seq.tryFind (fun v -> v.Name = "Message")) with
            | None -> String.Empty
            | Some value -> value.Value }

    let Scan (stream: Stream, cancellationToken: CancellationToken) =
        task {
            use reader = new StreamReader(stream)
            let! content = reader.ReadToEndAsync(cancellationToken)
            return pattern.Matches content |> Seq.map parse
        }
