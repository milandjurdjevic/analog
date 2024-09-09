module Analog.Tests.ConfigTest

open Xunit
open FsUnit.Xunit
open Analog.Config

[<Fact>]
let ``map returns a non-empty map`` () = templates |> should not' <| be Empty
