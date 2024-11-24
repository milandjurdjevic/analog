module Analog.Tests.FilterTest

open FsUnit.Xunit
open Xunit
open Analog.Log.Filter

[<Fact>]
let ``parse equal`` () =
    let expected = Binary(Identifier "name", Equal, Literal(String "John"))

    match "name = 'John'" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error

[<Fact>]
let ``parse not equal`` () =
    let expected = Binary(Identifier "age", NotEqual, Literal(Integer 30))

    match "age <> 30" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error

[<Fact>]
let ``parse greater than`` () =
    let expected = Binary(Identifier "score", GreaterThan, Literal(Float 75.5))

    match "score > 75.5" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error

[<Fact>]
let ``parse less than or equal`` () =
    let expected = Binary(Identifier "height", LessThanOrEqual, Literal(Integer 180))

    match "height <= 180" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error

[<Fact>]
let ``parse and`` () =
    let expected =
        Binary(
            Binary(Identifier "active", Equal, Literal(Boolean true)),
            And,
            Binary(Identifier "verified", Equal, Literal(Boolean true))
        )

    match "active = true & verified = true" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error

[<Fact>]
let ``parse or`` () =
    let expected =
        Binary(
            Binary(Identifier "role", Equal, Literal(String "admin")),
            Or,
            Binary(Identifier "role", Equal, Literal(String "user"))
        )

    match "role = 'admin' | role = 'user'" |> parse with
    | Ok expression -> expression |> should equal expected
    | Error error -> failwith error
