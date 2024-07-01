module Analog.Tests.TemplatesTest

open System.IO
open System.Text.RegularExpressions
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``toMap returns a non-empty map`` () =
    let templates = Analog.Templates.toMap ()
    templates |> should not' <| be Empty
    
[<Fact>]
let ``toIterable returns an iterable with a multiline regex`` () =
    let template = Analog.Templates.Template()
    let iterable = template |> Analog.Templates.toIterable Stream.Null 
    iterable.Regex.Options |> should equal RegexOptions.Multiline