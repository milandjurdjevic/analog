namespace Analog.Cli

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization
open System.Text.RegularExpressions
open System.Threading
open FSharp.Control
open System.Linq
open Microsoft.FSharp.Collections

type DimensionType =
    | String = 0
    | Number = 1
    | Timestamp = 2

type Dimension =
    { Name: string
      Type: DimensionType }

    member this.Parse(value: string) : obj option =
        match this.Type with
        | DimensionType.String -> Some(value :> obj)
        | DimensionType.Number -> Dimension.ParseNumber value
        | DimensionType.Timestamp -> Dimension.ParseTimestamp value
        | _ -> None

    static member private ParseTimestamp(value: string) =
        let success, parsedValue = DateTimeOffset.TryParse value
        if success then Some(parsedValue :> obj) else None

    static member private ParseNumber(value: string) =
        let success, parsedValue = Double.TryParse value
        if success then Some(parsedValue :> obj) else None

module private String =
    let ofCharMemory (memory: Memory<char>) =
        memory.ToArray()
        |> Array.filter (fun char -> char <> Unchecked.defaultof<char>)
        |> String

module private Regex =
    let toMap (groups: GroupCollection) =
        groups
        |> Seq.filter (fun object -> object.GetType() = typeof<Group>)
        |> Seq.map (fun group -> group.Name, group.Value)
        |> Map.ofSeq

type Template =
    { Name: string
      Regex: string
      Dimensions: Dimension list }

    member this.Parse (stream: Stream) (cancellationToken: CancellationToken) =
        taskSeq {
            use reader = new StreamReader(stream, Encoding.UTF8)
            let batchSizeMax = 1000
            let mutable batchSize = batchSizeMax
            let mutable leftover = String.Empty

            let regex =
                Regex(this.Regex, RegexOptions.Compiled ||| RegexOptions.Multiline ||| RegexOptions.IgnoreCase)

            while batchSize = batchSizeMax && not cancellationToken.IsCancellationRequested do
                let memory = Memory<char>(Array.zeroCreate batchSizeMax)
                let! blockSize = reader.ReadBlockAsync(memory, cancellationToken)
                batchSize <- blockSize

                let input = leftover + String.ofCharMemory memory
                let matches = regex.Matches input |> Array.ofSeq

                for regexMatch in matches.SkipLast 1 do
                    yield regexMatch |> (_.Groups) |> Regex.toMap |> this.TransformDimensions

                leftover <-
                    match matches |> Seq.tryLast with
                    | None -> String.Empty
                    | Some value -> input[value.Index ..]

                if batchSize < batchSizeMax then
                    for regexMatch in regex.Matches leftover do
                        yield regexMatch |> (_.Groups) |> Regex.toMap |> this.TransformDimensions
        }

    member private this.TransformDimensions(map: Map<string, string>) =
        let parseValue (dimension: Dimension) =
            map.TryFind dimension.Name
            |> Option.map dimension.Parse
            |> Option.flatten
            |> Option.defaultValue String.Empty

        this.Dimensions
        |> List.map (fun dimension -> dimension.Name, parseValue dimension)
        |> readOnlyDict

    static member Import() =
        let path = Path.Combine [| Directory.GetCurrentDirectory(); "templates.json" |]
        use stream = File.OpenRead path
        let json = JsonSerializer.Deserialize<JsonObject> stream
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.Converters.Add(JsonStringEnumConverter())
        json["templates"].Deserialize<IEnumerable<Template>> options
