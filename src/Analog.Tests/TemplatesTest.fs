module Analog.Tests.TemplatesTest

open System.IO
open System.Text
open System.Text.RegularExpressions
open Xunit
open FsUnit.Xunit
open Analog.Templates

[<Fact>]
let ``toMap returns a non-empty map`` () = templateMap |> should not' <| be Empty

[<Fact>]
let ``toIterable returns an iterable with a multiline regex`` () =
    let template = Template()
    let iterable = template |> toIterable Stream.Null
    iterable.Regex.Options |> should equal RegexOptions.Multiline

[<Fact>]
let ``iter iterates through all logs`` () =
    let bytes = Fixture.logs |> Encoding.UTF8.GetBytes
    use stream = new MemoryStream(bytes)
    let mutable counter = 0

    templateMap
    |> Seq.head
    |> _.Value
    |> toIterable stream
    |> iter (fun _ -> counter <- counter + 1)

    counter |> should equal 41
