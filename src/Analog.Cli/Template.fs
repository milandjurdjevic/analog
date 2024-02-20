namespace Analog.Cli

open System
open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization

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

type Template =
    { Name: string
      Regex: string
      Dimensions: Dimension list }

    static member Init() =
        let path = Path.Combine [| Directory.GetCurrentDirectory(); "templates.json" |]
        use stream = File.OpenRead path
        let json = JsonSerializer.Deserialize<JsonObject> stream
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.Converters.Add(JsonStringEnumConverter())
        json["templates"].Deserialize<IEnumerable<Template>> options
