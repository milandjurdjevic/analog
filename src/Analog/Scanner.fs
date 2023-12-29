namespace Analog

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Control
open Microsoft.FSharp.Core

module Scanner =
    let private pattern =
        Regex(
            "^(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})? (?<Severity>\[[A-Z]{3}\]) (?<Message>[\s\S]*?\n*(?=^[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
            RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
        )

    let private toSeverityFromString (value: string) =
        match value with
        | "[CRT]" -> Severity.Critical
        | "[ERR]" -> Severity.Error
        | "[WRN]" -> Severity.Warning
        | "[INF]" -> Severity.Information
        | "[DBG]" -> Severity.Debug
        | _ -> Severity.Trace

    let private toTimestampFromString (value: string) =
        let parsed, parsedResult = DateTimeOffset.TryParse value
        if parsed then parsedResult else DateTimeOffset.MinValue

    let private toGroupDictFromMatch (capture: Match) =
        capture.Groups
        |> Seq.filter (fun group -> group.GetType() = typeof<Group>)
        |> Seq.map (fun group -> group.Name, group.Value)
        |> readOnlyDict

    let private toOptionalValueFromDict (dict: IReadOnlyDictionary<string, string>, key: string) =
        let exists, value = dict.TryGetValue key
        if exists then Some value else None

    let private toLogFromDict (payload: IReadOnlyDictionary<string, string>) =
        { Severity =
            match toOptionalValueFromDict (payload, "Severity") with
            | Some value -> toSeverityFromString value
            | None -> Severity.Trace
          Timestamp =
            match toOptionalValueFromDict (payload, "Timestamp") with
            | Some value -> toTimestampFromString value
            | None -> DateTimeOffset.MinValue
          Message =
            match toOptionalValueFromDict (payload, "Message") with
            | Some value -> value
            | None -> String.Empty }

    let Scan (stream: Stream, cancellationToken: CancellationToken) : Task<Log seq> =
        task {
            use reader = new StreamReader(stream)
            let! content = reader.ReadToEndAsync(cancellationToken)
            return pattern.Matches content |> Seq.map toGroupDictFromMatch |> Seq.map toLogFromDict
        }
