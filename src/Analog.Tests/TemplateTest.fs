module Analog.Tests.TemplateTest

open Xunit
open FsUnit.Xunit
open Analog.Template

[<Fact>]
let ``map returns a non-empty map`` () = configuration |> should not' <| be Empty
