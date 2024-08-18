module Analog.Tests.FilterTest

open Analog.Filter
open Xunit

[<Theory>]
[<InlineData("name = 'value'")>]
[<InlineData("has18 = true | age > 17")>]
[<InlineData("has18 == true | age > 17")>]
let ``Parse valid expression`` input =
    match Parser.parse input with
    | Ok value -> value.ToString() |> Snapshot.compare |> Snapshot.input [|input|] |> Snapshot.exec
    | Error error -> invalidOp error
    
