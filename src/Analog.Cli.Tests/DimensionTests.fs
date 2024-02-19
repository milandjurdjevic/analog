module Analog.Cli.Tests.DimensionTests

open System
open Analog.Cli
open Xunit

[<Theory>]
[<InlineData(DimensionType.String, "test")>]
[<InlineData(DimensionType.Number, "1")>]
[<InlineData(DimensionType.Timestamp, "2024-01-01")>]
let ``Parse returns some if type is correct`` (dimensionType: DimensionType) (value: string) =
    { Name = ""; Type = dimensionType }.Parse value |> _.IsSome |> Assert.True
    
[<Theory>]
[<InlineData(DimensionType.Number, "test")>]
[<InlineData(DimensionType.Timestamp, "test")>]
let ``Parse returns none if value cannot be parsed`` (dimensionType: DimensionType) (value: string) =
    { Name = ""; Type = dimensionType }.Parse value |> _.IsNone |> Assert.True

[<Theory>]
[<InlineData(Byte.MaxValue, "test")>]
let ``Parse returns none when type is not correct`` (dimensionType: DimensionType) (value: string) =
    { Name = ""; Type = dimensionType }.Parse value |> _.IsNone |> Assert.True
