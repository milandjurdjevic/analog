module Analog.Tests.FilterParserTest

open System

open FsUnitTyped
open Xunit

open Analog

let parse = FilterParser.expression |> ParserRunner.run

[<Fact>]
let ``parse should correctly parse a constant string`` () =
    parse "'hello'" |> shouldEqual (Result.Ok(Const(String "hello")))

[<Fact>]
let ``parse should correctly parse a constant number`` () =
    parse "42.5" |> shouldEqual (Result.Ok(Const(Literal.Number 42.5)))

[<Fact>]
let ``parse should correctly parse a constant boolean (true)`` () =
    parse "true" |> shouldEqual (Result.Ok(Const(Boolean true)))

[<Fact>]
let ``parse should correctly parse a constant boolean (false)`` () =
    parse "false" |> shouldEqual (Result.Ok(Const(Boolean false)))

[<Fact>]
let ``parse should correctly parse a field identifier`` () =
    parse "fieldName" |> shouldEqual (Result.Ok(Field "fieldName"))

[<Fact>]
let ``parse should correctly parse a simple binary expression`` () =
    parse "'value' = fieldName"
    |> shouldEqual (Result.Ok(Binary(Const(String "value"), Equal, Field "fieldName")))

[<Fact>]
let ``parse should correctly parse a complex binary expression`` () =
    parse "'value' = fieldName & 42 > 10"
    |> shouldEqual (
        Result.Ok(
            Binary(
                Binary(Const(String "value"), Equal, Field "fieldName"),
                And,
                Binary(Const(Literal.Number 42.0), GreaterThan, Const(Literal.Number 10.0))
            )
        )
    )

[<Fact>]
let ``parse should correctly parse a timestamp`` () =
    let input = "2024-01-03T12:34:56+00:00"
    let result = parse input
    result |> shouldEqual (Result.Ok(Const(Timestamp(DateTimeOffset.Parse input))))

[<Fact>]
let ``parse should return an error for invalid input`` () =
    match parse "'unterminated string" with
    | Ok _ -> failwith "parse should return an error for invalid input"
    | Error _ -> ()
