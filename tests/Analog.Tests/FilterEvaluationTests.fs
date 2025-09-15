module Analog.Tests.FilterEvaluationTests

open System
open Expecto
open Analog.Logs
open Analog.Filters

let createLogEntry (fields: (string * LogLiteral) list) = fields |> Map.ofList |> wrapLogEntry

let createStringEntry key value =
    createLogEntry [ (key, StringLiteral value) ]

let createNumberEntry key value =
    createLogEntry [ (key, NumberLiteral value) ]

let createBoolEntry key value =
    createLogEntry [ (key, BooleanLiteral value) ]

let createTimestampEntry key value =
    createLogEntry [ (key, TimestampLiteral value) ]

let sampleTimestamp = DateTimeOffset(2025, 8, 8, 10, 30, 0, TimeSpan.Zero)
let laterTimestamp = DateTimeOffset(2025, 8, 8, 11, 30, 0, TimeSpan.Zero)

let literalTests =
    testCase "LiteralFilterExpression always evaluates to true"
    <| fun _ ->
        let entry = createStringEntry "level" "INFO"
        let expression = LiteralExpression(StringLiteral "test")
        let result = applyFilter expression entry
        Expect.isTrue result "Literal expression should always evaluate to true"

let memberTests =
    testList
        "Member expression tests"
        [ testCase "MemberFilterExpression returns true when field exists"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"
              let expression = MemberExpression "level"
              let result = applyFilter expression entry
              Expect.isTrue result "Existing member should return true"
          testCase "MemberFilterExpression returns false when field does not exist"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"
              let expression = MemberExpression "missing"
              let result = applyFilter expression entry
              Expect.isFalse result "Missing member should return false" ]

let equalityTests =
    testList
        "Equality operator tests"
        [ testCase "EqualFilterOperator works with string literals"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"

              let expression =
                  BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "INFO"))

              Expect.isTrue (applyFilter expression entry) "Strings should be equal"
          testCase "EqualFilterOperator returns false for different strings"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"

              let expression =
                  BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))

              Expect.isFalse (applyFilter expression entry) "Different strings should not be equal"
          testCase "EqualFilterOperator works with numbers"
          <| fun _ ->
              let entry = createNumberEntry "count" 42.0

              let expression =
                  BinaryExpression(MemberExpression "count", EqualOperator, LiteralExpression(NumberLiteral 42.0))

              Expect.isTrue (applyFilter expression entry) "Numbers should be equal"
          testCase "EqualFilterOperator works with booleans"
          <| fun _ ->
              let entry = createBoolEntry "active" true

              let expression =
                  BinaryExpression(MemberExpression "active", EqualOperator, LiteralExpression(BooleanLiteral true))

              Expect.isTrue (applyFilter expression entry) "Booleans should be equal"
          testCase "EqualFilterOperator works with timestamps"
          <| fun _ ->
              let entry = createTimestampEntry "timestamp" sampleTimestamp

              let expression =
                  BinaryExpression(
                      MemberExpression "timestamp",
                      EqualOperator,
                      LiteralExpression(TimestampLiteral sampleTimestamp)
                  )

              Expect.isTrue (applyFilter expression entry) "Timestamps should be equal" ]

let comparisonTests =
    testList
        "Comparison operator tests"
        [ testCase "GreaterFilterOperator works with numbers"
          <| fun _ ->
              let entry = createNumberEntry "count" 50.0

              let expression =
                  BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 42.0))

              Expect.isTrue (applyFilter expression entry) "> should compare numbers"
          testCase "GreaterFilterOperator returns false when left is smaller"
          <| fun _ ->
              let entry = createNumberEntry "count" 30.0

              let expression =
                  BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 42.0))

              Expect.isFalse (applyFilter expression entry) "Should be false when left < right"
          testCase "GreaterFilterOperator works with strings"
          <| fun _ ->
              let entry = createStringEntry "name" "charlie"

              let expression =
                  BinaryExpression(MemberExpression "name", GreaterOperator, LiteralExpression(StringLiteral "alice"))

              Expect.isTrue (applyFilter expression entry) "String comparison should work"
          testCase "GreaterFilterOperator works with timestamps"
          <| fun _ ->
              let entry = createTimestampEntry "timestamp" laterTimestamp

              let expression =
                  BinaryExpression(
                      MemberExpression "timestamp",
                      GreaterOperator,
                      LiteralExpression(TimestampLiteral sampleTimestamp)
                  )

              Expect.isTrue (applyFilter expression entry) "> should compare timestamps"
          testCase "LessFilterOperator works with numbers"
          <| fun _ ->
              let entry = createNumberEntry "count" 30.0

              let expression =
                  BinaryExpression(MemberExpression "count", LessOperator, LiteralExpression(NumberLiteral 42.0))

              Expect.isTrue (applyFilter expression entry) "< should compare numbers" ]

let stringOpTests =
    testList
        "String operator tests"
        [ testCase "ContainFilterOperator works with strings"
          <| fun _ ->
              let entry = createStringEntry "message" "This is an error message"

              let expression =
                  BinaryExpression(
                      MemberExpression "message",
                      ContainOperator,
                      LiteralExpression(StringLiteral "error")
                  )

              Expect.isTrue (applyFilter expression entry) "Substring should be found"
          testCase "ContainFilterOperator returns false when substring not found"
          <| fun _ ->
              let entry = createStringEntry "message" "This is an info message"

              let expression =
                  BinaryExpression(
                      MemberExpression "message",
                      ContainOperator,
                      LiteralExpression(StringLiteral "error")
                  )

              Expect.isFalse (applyFilter expression entry) "Substring should not be found"
          testCase "StartFilterOperator works with strings"
          <| fun _ ->
              let entry = createStringEntry "message" "Error: Something went wrong"

              let expression =
                  BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Error:"))

              Expect.isTrue (applyFilter expression entry) "Should start with prefix"
          testCase "StartFilterOperator returns false when string doesn't start with prefix"
          <| fun _ ->
              let entry = createStringEntry "message" "Info: Something happened"

              let expression =
                  BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Error:"))

              Expect.isFalse (applyFilter expression entry) "Should not start with prefix"
          testCase "EndFilterOperator works with strings"
          <| fun _ ->
              let entry = createStringEntry "file" "application.log"

              let expression =
                  BinaryExpression(MemberExpression "file", EndOperator, LiteralExpression(StringLiteral ".log"))

              Expect.isTrue (applyFilter expression entry) "Should end with suffix"
          testCase "EndFilterOperator returns false when string doesn't end with suffix"
          <| fun _ ->
              let entry = createStringEntry "file" "application.txt"

              let expression =
                  BinaryExpression(MemberExpression "file", EndOperator, LiteralExpression(StringLiteral ".log"))

              Expect.isFalse (applyFilter expression entry) "Should not end with suffix" ]

let logicTests =
    testList
        "Logical operator tests"
        [ testCase "AndFilterOperator returns true when both operands are true"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 50.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      AndOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isTrue (applyFilter expression entry) "Both sides true"
          testCase "AndFilterOperator returns false when left operand is false"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 50.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      AndOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isFalse (applyFilter expression entry) "Left false should short-circuit"
          testCase "AndFilterOperator returns false when right operand is false"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 20.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      AndOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isFalse (applyFilter expression entry) "Right false"
          testCase "OrFilterOperator returns true when left operand is true"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 20.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      OrOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isTrue (applyFilter expression entry) "Left true should short-circuit"
          testCase "OrFilterOperator returns true when right operand is true"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 50.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      OrOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isTrue (applyFilter expression entry) "Right true"
          testCase "OrFilterOperator returns false when both operands are false"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 20.0) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      ),
                      OrOperator,
                      BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
                  )

              Expect.isFalse (applyFilter expression entry) "Both false"
          testCase "NotFilterOperator negates the result"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"

              let expression =
                  BinaryExpression(
                      LiteralExpression(StringLiteral "dummy"),
                      NotOperator,
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      )
                  )

              Expect.isTrue (applyFilter expression entry) "Not false should be true"
          testCase "NotFilterOperator with true expression returns false"
          <| fun _ ->
              let entry = createStringEntry "level" "ERROR"

              let expression =
                  BinaryExpression(
                      LiteralExpression(StringLiteral "dummy"),
                      NotOperator,
                      BinaryExpression(
                          MemberExpression "level",
                          EqualOperator,
                          LiteralExpression(StringLiteral "ERROR")
                      )
                  )

              Expect.isFalse (applyFilter expression entry) "Not true should be false" ]

let mismatchAndEdgeTests =
    testList
        "Mismatch & edge cases"
        [ testCase "Comparison returns false for type mismatches"
          <| fun _ ->
              let entry =
                  createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 42.0) ]

              let expression =
                  BinaryExpression(MemberExpression "level", GreaterOperator, MemberExpression "count")

              Expect.isFalse (applyFilter expression entry) "Type mismatch should be false"
          testCase "Comparison returns false when field doesn't exist"
          <| fun _ ->
              let entry = createStringEntry "level" "INFO"

              let expression =
                  BinaryExpression(
                      MemberExpression "missing_field",
                      EqualOperator,
                      LiteralExpression(StringLiteral "INFO")
                  )

              Expect.isFalse (applyFilter expression entry) "Missing field should be false"
          testCase "String operations return false for non-string types"
          <| fun _ ->
              let entry = createNumberEntry "count" 42.0

              let expression =
                  BinaryExpression(MemberExpression "count", ContainOperator, LiteralExpression(StringLiteral "4"))

              Expect.isFalse (applyFilter expression entry) "Contain on number should be false"
          testCase "Complex nested expression evaluation"
          <| fun _ ->
              let entry =
                  createLogEntry
                      [ ("level", StringLiteral "ERROR")
                        ("count", NumberLiteral 50.0)
                        ("active", BooleanLiteral true) ]

              let expression =
                  BinaryExpression(
                      BinaryExpression(
                          BinaryExpression(
                              MemberExpression "level",
                              EqualOperator,
                              LiteralExpression(StringLiteral "ERROR")
                          ),
                          AndOperator,
                          BinaryExpression(
                              MemberExpression "count",
                              GreaterOperator,
                              LiteralExpression(NumberLiteral 30.0)
                          )
                      ),
                      OrOperator,
                      BinaryExpression(
                          MemberExpression "active",
                          EqualOperator,
                          LiteralExpression(BooleanLiteral false)
                      )
                  )

              Expect.isTrue (applyFilter expression entry) "Complex nesting result"
          testCase "Filter expression parsing and evaluation integration"
          <| fun _ ->
              let entry = createStringEntry "level" "ERROR"

              let simpleExpression =
                  BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))

              Expect.isTrue (applyFilter simpleExpression entry) "Integration simple expression" ]

let allFilterEvaluationTests =
    testList
        "Filter evaluation"
        [ literalTests
          memberTests
          equalityTests
          comparisonTests
          stringOpTests
          logicTests
          mismatchAndEdgeTests ]

// Expose list for aggregation in TestEntry
let tests = allFilterEvaluationTests
