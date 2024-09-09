module Analog.Tests.TemplateTest

open System
open Analog
open Template
open Xunit

[<Theory>]
[<InlineData("{message:string}")>]
let ``Parse template from string`` input =
    match Parser.parse input with
    | Ok result ->
        result.ToString()
        |> Snapshot.cmp
        |> Snapshot.id (Guid "5d62f0b6301f49809c63cf469977a653")
        |> Snapshot.arg [ input ]
        |> Snapshot.run
    | Error error -> invalidOp error
