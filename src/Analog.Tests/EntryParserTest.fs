module Analog.Tests.EntryParserTest

open System
open Analog
open FsUnit.Xunit
open Xunit

let parse txt =
    EntryParser.value |> EntryParser.parse txt

let stringOf = Literal.String
let timestampOf = DateTimeOffset.Parse >> Literal.Timestamp

[<Fact>]
let ``parse single log line`` () =
    let expected =
        [ "loglevel", stringOf "INFO"
          "message", stringOf "User logged in successfully"
          "timestamp", timestampOf "2024-11-17 14:30:55" ]
        |> Map.ofList
        |> List.singleton

    "[2024-11-17 14:30:55] [INFO] User logged in successfully"
    |> parse
    |> should equal expected

[<Fact>]
let ``parse two log lines`` () =
    let expected =
        [ [ "loglevel", stringOf "INFO"
            "message", stringOf "User logged in successfully"
            "timestamp", timestampOf "2024-11-17 14:30:55" ]
          |> Map.ofList
          [ "loglevel", stringOf "ERROR"
            "message", stringOf "Failed to authenticate user"
            "timestamp", timestampOf "2024-11-17 14:31:00" ]
          |> Map.ofList ]
        |> List.ofSeq

    "[2024-11-17 14:30:55] [INFO] User logged in successfully\n[2024-11-17 14:31:00] [ERROR] Failed to authenticate user"
    |> parse
    |> should equal expected
