module Analog.Tests.TemplateTest

open System.IO
open System.Text
open Analog
open FsUnit.Xunit
open GrokNet
open Xunit
open Template

[<Fact>]
let ``load configured templates`` () = load () |> should not' Empty

// Grok pattern for tests.
let grok =
    Grok("\[%{TIMESTAMP_ISO8601:timestamp}\] \[%{LOGLEVEL:loglevel}\] %{GREEDYDATA:message}")

[<Fact>]
let ``parse single log line`` () =
    let expected =
        [ "loglevel", box "INFO"
          "message", box "User logged in successfully"
          "timestamp", box "2024-11-17 14:30:55" ]
        |> Map.ofList
        |> List.singleton

    "[2024-11-17 14:30:55] [INFO] User logged in successfully"
    |> Encoding.UTF8.GetBytes
    |> fun bytes -> new MemoryStream(bytes)
    |> parse grok
    |> should equal expected

[<Fact>]
let ``parse two log lines`` () =
    let expected =
        [ [ "loglevel", box "INFO"
            "message", box "User logged in successfully"
            "timestamp", box "2024-11-17 14:30:55" ]
          |> Map.ofList
          [ "loglevel", box "ERROR"
            "message", box "Failed to authenticate user"
            "timestamp", box "2024-11-17 14:31:00" ]
          |> Map.ofList ]
        |> List.ofSeq

    "[2024-11-17 14:30:55] [INFO] User logged in successfully\n[2024-11-17 14:31:00] [ERROR] Failed to authenticate user"
    |> Encoding.UTF8.GetBytes
    |> fun bytes -> new MemoryStream(bytes)
    |> parse grok
    |> should equal expected
