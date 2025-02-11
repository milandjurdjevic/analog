module Analog.Core.Tests.GrokTest

open System
open Analog.Core
open Swensen.Unquote
open Xunit

open Log

let parse txt =
    Grok.pattern
    |> Grok.extract txt
    |> Result.map (Grok.group >> Grok.transform)
    |> function
        | Ok resultValue -> resultValue
        | Error errorValue -> failwith errorValue

let stringOf = StringLiteral
let timestampOf = DateTimeOffset.Parse >> TimestampLiteral

[<Fact>]
let ``parse single log line`` () =
    let expected =
        [ "loglevel", stringOf "INFO"
          "message", stringOf "User logged in successfully"
          "timestamp", timestampOf "2024-11-17 14:30:55" ]
        |> Map.ofList
        |> Entry
        |> List.singleton

    let actual = parse "[2024-11-17 14:30:55] [INFO] User logged in successfully"

    test <@ actual = expected @>

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
        |> List.map Entry

    let actual =
        parse
            "[2024-11-17 14:30:55] [INFO] User logged in successfully\n[2024-11-17 14:31:00] [ERROR] Failed to authenticate user"

    test <@ actual = expected @>
