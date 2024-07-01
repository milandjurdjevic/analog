module Analog.Tests.TemplatesTest

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``toMap returns a non-empty map`` () =
    let templates = Analog.Templates.toMap ()
    templates |> should not' <| be Empty