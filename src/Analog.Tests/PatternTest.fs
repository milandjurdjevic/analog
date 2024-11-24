module Analog.Tests.PatternTest

open FsUnit.Xunit
open GrokNet
open Xunit

let parse =
    Grok("\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}")
    |> Analog.Log.Pattern.extract

[<Fact>]
let ``parse single log line`` () =
    let expected =
        [ "loglevel", "INFO"
          "message", "User logged in successfully"
          "timestamp", "2024-11-17 14:30:55" ]
        |> Map.ofList
        |> List.singleton

    "[2024-11-17 14:30:55] [INFO] User logged in successfully"
    |> parse
    |> should equal expected

[<Fact>]
let ``parse two log lines`` () =
    let expected =
        [ [ "loglevel", "INFO"
            "message", "User logged in successfully"
            "timestamp", "2024-11-17 14:30:55" ]
          |> Map.ofList
          [ "loglevel", "ERROR"
            "message", "Failed to authenticate user"
            "timestamp", "2024-11-17 14:31:00" ]
          |> Map.ofList ]
        |> List.ofSeq

    "[2024-11-17 14:30:55] [INFO] User logged in successfully\n[2024-11-17 14:31:00] [ERROR] Failed to authenticate user"
    |> parse
    |> should equal expected
