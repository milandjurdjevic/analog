module Analog.Tests.AdditionalTests

open System
open Expecto
open Analog.Args
open Analog.Logs
open Analog.Filters

let argsTests =
    testList
        "Args parsing"
        [ testCase "parseArgs single input"
          <| fun _ ->
              match parseArgs [| "file1.log" |] with
              | Ok a -> Expect.sequenceEqual a.Inputs [ "file1.log" ] "Should capture input"
              | Error e -> failtestf "Expected Ok but got %s" e
          testCase "parseArgs with output and pattern"
          <| fun _ ->
              match parseArgs [| "file.log"; "-o"; "out.json"; "-p"; "{ts:timestamp}" |] with
              | Ok a ->
                  Expect.equal a.Output (Some "out.json") "Output option"
                  Expect.equal a.Pattern (Some "{ts:timestamp}") "Pattern option"
              | Error e -> failtestf "Unexpected error %s" e
          testCase "parseArgs missing mandatory shows error"
          <| fun _ ->
              match parseArgs [||] with
              | Ok _ -> failtest "Expected error"
              | Error _ -> () ]

let logParsingTests =
    testList
        "Log pattern parsing"
        [ testCase "parseLog simple auto fields"
          <| fun _ ->
              let pattern = "{level:string} {msg:string}"
              let line = "INFO Started"

              match parseLog pattern line with
              | Ok entry ->
                  let m = unwrapLogEntry entry
                  Expect.equal (Map.count m) 2 "Two fields"
                  Expect.isTrue (m.ContainsKey "level") "level exists"
                  Expect.isTrue (m.ContainsKey "msg") "msg exists"
              | Error e -> failtestf "Unexpected error %s" e
          testCase "parseLog timestamp field"
          <| fun _ ->
              let pattern = "{ts:timestamp} rest"
              let line = "2025-08-08T10:30:00Z rest"

              match parseLog pattern line with
              | Ok entry ->
                  let ts = unwrapLogEntry entry |> Map.find "ts"

                  match ts with
                  | TimestampLiteral _ -> ()
                  | other -> failtestf "Expected timestamp got %A" other
              | Error e -> failtestf "Unexpected error %s" e ]

let logConversionTests =
    testCase "convertLogEntryToObject produces readonly dict"
    <| fun _ ->
        let entry =
            LogEntry(Map.ofList [ "level", StringLiteral "INFO"; "count", NumberLiteral 1.0 ])

        let objVal = convertLogEntryToObject entry
        Expect.isNotNull objVal "Should return object"

let parseFilterTests =
    testList
        "Filter evaluation extra"
        [ testCase "NotOperator chooses first non-literal"
          <| fun _ ->
              // Build an expression where left is literal and right is member to ensure extractNotOperand picks right
              let entry = LogEntry(Map.ofList [ "level", StringLiteral "INFO" ])

              let expr =
                  BinaryExpression(LiteralExpression(StringLiteral "ignored"), NotOperator, MemberExpression "level")
              // MemberExpression alone would evaluate to true (exists), NotOperator should negate -> false
              Expect.isFalse (applyFilter expr entry) "Negated member existence"
          testCase "NotOperator second path"
          <| fun _ ->
              let entry = LogEntry(Map.ofList [ "level", StringLiteral "INFO" ])
              // Place member on left so extractNotOperand returns left
              let expr =
                  BinaryExpression(MemberExpression "level", NotOperator, LiteralExpression(StringLiteral "ignored"))

              Expect.isFalse (applyFilter expr entry) "Negated member existence (left)" ]

let tests =
    testList "Additional" [ argsTests; logParsingTests; logConversionTests; parseFilterTests ]
