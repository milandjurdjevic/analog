namespace Analog.Cli

open System

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
        | _ -> Option.None

    static private ParseTimestamp (value: string) =
        let success, parsedValue = DateTimeOffset.TryParse value
        if success then Some(parsedValue :> obj) else Option.None

    static private ParseNumber (value: string) =
        let success, parsedValue = Double.TryParse value
        if success then Some(parsedValue :> obj) else Option.None
