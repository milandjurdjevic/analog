module Analog.Tests.FilterEvaluationTests

open System
open Xunit
open Analog.Logs
open Analog.Filters

let createLogEntry (fields: (string * Literal) list) = fields |> Map.ofList |> Entry.create

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

[<Fact>]
let ``LiteralFilterExpression always evaluates to true`` () =
    let entry = createStringEntry "level" "INFO"
    let expression = LiteralExpression(StringLiteral "test")

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``MemberFilterExpression returns true when field exists`` () =
    let entry = createStringEntry "level" "INFO"
    let expression = MemberExpression "level"

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``MemberFilterExpression returns false when field does not exist`` () =
    let entry = createStringEntry "level" "INFO"
    let expression = MemberExpression "missing"

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``EqualFilterOperator works with string literals`` () =
    let entry = createStringEntry "level" "INFO"

    let expression =
        BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "INFO"))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``EqualFilterOperator returns false for different strings`` () =
    let entry = createStringEntry "level" "INFO"

    let expression =
        BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``EqualFilterOperator works with numbers`` () =
    let entry = createNumberEntry "count" 42.0

    let expression =
        BinaryExpression(MemberExpression "count", EqualOperator, LiteralExpression(NumberLiteral 42.0))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``EqualFilterOperator works with booleans`` () =
    let entry = createBoolEntry "active" true

    let expression =
        BinaryExpression(MemberExpression "active", EqualOperator, LiteralExpression(BooleanLiteral true))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``EqualFilterOperator works with timestamps`` () =
    let entry = createTimestampEntry "timestamp" sampleTimestamp

    let expression =
        BinaryExpression(
            MemberExpression "timestamp",
            EqualOperator,
            LiteralExpression(TimestampLiteral sampleTimestamp)
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``GreaterFilterOperator works with numbers`` () =
    let entry = createNumberEntry "count" 50.0

    let expression =
        BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 42.0))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``GreaterFilterOperator returns false when left is smaller`` () =
    let entry = createNumberEntry "count" 30.0

    let expression =
        BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 42.0))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``GreaterFilterOperator works with strings`` () =
    let entry = createStringEntry "name" "charlie"

    let expression =
        BinaryExpression(MemberExpression "name", GreaterOperator, LiteralExpression(StringLiteral "alice"))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``GreaterFilterOperator works with timestamps`` () =
    let entry = createTimestampEntry "timestamp" laterTimestamp

    let expression =
        BinaryExpression(
            MemberExpression "timestamp",
            GreaterOperator,
            LiteralExpression(TimestampLiteral sampleTimestamp)
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``LessFilterOperator works with numbers`` () =
    let entry = createNumberEntry "count" 30.0

    let expression =
        BinaryExpression(MemberExpression "count", LessOperator, LiteralExpression(NumberLiteral 42.0))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``ContainFilterOperator works with strings`` () =
    let entry = createStringEntry "message" "This is an error message"

    let expression =
        BinaryExpression(MemberExpression "message", ContainOperator, LiteralExpression(StringLiteral "error"))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``ContainFilterOperator returns false when substring not found`` () =
    let entry = createStringEntry "message" "This is an info message"

    let expression =
        BinaryExpression(MemberExpression "message", ContainOperator, LiteralExpression(StringLiteral "error"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``StartFilterOperator works with strings`` () =
    let entry = createStringEntry "message" "Error: Something went wrong"

    let expression =
        BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Error:"))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``StartFilterOperator returns false when string doesn't start with prefix`` () =
    let entry = createStringEntry "message" "Info: Something happened"

    let expression =
        BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Error:"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``EndFilterOperator works with strings`` () =
    let entry = createStringEntry "file" "application.log"

    let expression =
        BinaryExpression(MemberExpression "file", EndOperator, LiteralExpression(StringLiteral ".log"))

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``EndFilterOperator returns false when string doesn't end with suffix`` () =
    let entry = createStringEntry "file" "application.txt"

    let expression =
        BinaryExpression(MemberExpression "file", EndOperator, LiteralExpression(StringLiteral ".log"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``AndFilterOperator returns true when both operands are true`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 50.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            AndOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``AndFilterOperator returns false when left operand is false`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 50.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            AndOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``AndFilterOperator returns false when right operand is false`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 20.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            AndOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``OrFilterOperator returns true when left operand is true`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "ERROR"); ("count", NumberLiteral 20.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            OrOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``OrFilterOperator returns true when right operand is true`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 50.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            OrOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``OrFilterOperator returns false when both operands are false`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 20.0) ]

    let expression =
        BinaryExpression(
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
            OrOperator,
            BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
        )

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``NotFilterOperator negates the result`` () =
    let entry = createStringEntry "level" "INFO"

    let expression =
        BinaryExpression(
            LiteralExpression(StringLiteral "dummy"),
            NotOperator,
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``NotFilterOperator with true expression returns false`` () =
    let entry = createStringEntry "level" "ERROR"

    let expression =
        BinaryExpression(
            LiteralExpression(StringLiteral "dummy"),
            NotOperator,
            BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))
        )

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``Comparison returns false for type mismatches`` () =
    let entry =
        createLogEntry [ ("level", StringLiteral "INFO"); ("count", NumberLiteral 42.0) ]

    let expression =
        BinaryExpression(MemberExpression "level", GreaterOperator, MemberExpression "count")

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``Comparison returns false when field doesn't exist`` () =
    let entry = createStringEntry "level" "INFO"

    let expression =
        BinaryExpression(MemberExpression "missing_field", EqualOperator, LiteralExpression(StringLiteral "INFO"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``String operations return false for non-string types`` () =
    let entry = createNumberEntry "count" 42.0

    let expression =
        BinaryExpression(MemberExpression "count", ContainOperator, LiteralExpression(StringLiteral "4"))

    let result = Filter.eval expression entry

    Assert.False(result)

[<Fact>]
let ``Complex nested expression evaluation`` () =
    let entry =
        createLogEntry
            [ ("level", StringLiteral "ERROR")
              ("count", NumberLiteral 50.0)
              ("active", BooleanLiteral true) ]

    let expression =
        BinaryExpression(
            BinaryExpression(
                BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR")),
                AndOperator,
                BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 30.0))
            ),
            OrOperator,
            BinaryExpression(MemberExpression "active", EqualOperator, LiteralExpression(BooleanLiteral false))
        )

    let result = Filter.eval expression entry

    Assert.True(result)

[<Fact>]
let ``Filter expression parsing and evaluation integration`` () =
    let entry = createStringEntry "level" "ERROR"

    let simpleExpression =
        BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))

    let result = Filter.eval simpleExpression entry
    Assert.True(result)
