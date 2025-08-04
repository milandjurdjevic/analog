module Analog.Tests.FilterParserTests

open Xunit
open Analog.Logs
open Analog.Filters

[<Fact>]
let ``parseExpression should parse string literals`` () =
    let result = Filter.create "'hello'"

    match result with
    | Some(LiteralExpression(StringLiteral "hello")) -> ()
    | _ -> Assert.Fail("Expected string literal 'hello'")

[<Fact>]
let ``parseExpression should parse number literals`` () =
    let result = Filter.create "42.5"

    match result with
    | Some(LiteralExpression(NumberLiteral 42.5)) -> ()
    | _ -> Assert.Fail("Expected number literal 42.5")

[<Fact>]
let ``parseExpression should parse boolean literals`` () =
    let result = Filter.create "true"

    match result with
    | Some(LiteralExpression(BooleanLiteral true)) -> ()
    | _ -> Assert.Fail("Expected boolean literal true")

    let result2 = Filter.create "false"

    match result2 with
    | Some(LiteralExpression(BooleanLiteral false)) -> ()
    | _ -> Assert.Fail("Expected boolean literal false")

[<Fact>]
let ``parseExpression should parse member expressions`` () =
    let result = Filter.create "level"

    match result with
    | Some(MemberExpression "level") -> ()
    | _ -> Assert.Fail("Expected member expression 'level'")

    let result2 = Filter.create "message"

    match result2 with
    | Some(MemberExpression "message") -> ()
    | _ -> Assert.Fail("Expected member expression 'message'")

[<Fact>]
let ``parseExpression should parse equal operator`` () =
    let result = Filter.create "level = 'ERROR'"

    match result with
    | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) -> ()
    | _ -> Assert.Fail("Expected equal binary expression: level = 'ERROR'")

[<Fact>]
let ``parseExpression should parse greater than operator`` () =
    let result = Filter.create "count > 10"

    match result with
    | Some(BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 10.0))) -> ()
    | _ -> Assert.Fail("Expected greater than binary expression: count > 10")

[<Fact>]
let ``parseExpression should parse less than operator`` () =
    let result = Filter.create "count < 5"

    match result with
    | Some(BinaryExpression(MemberExpression "count", LessOperator, LiteralExpression(NumberLiteral 5.0))) -> ()
    | _ -> Assert.Fail("Expected less than binary expression: count < 5")

[<Fact>]
let ``parseExpression should parse contains operator`` () =
    let result = Filter.create "message ~ 'database'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", ContainOperator, LiteralExpression(StringLiteral "database"))) ->
        ()
    | _ -> Assert.Fail("Expected contains binary expression: message ~ 'database'")

[<Fact>]
let ``parseExpression should parse starts with operator`` () =
    let result = Filter.create "message ^ 'Starting'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Starting"))) ->
        ()
    | _ -> Assert.Fail("Expected starts with binary expression: message ^ 'Starting'")

[<Fact>]
let ``parseExpression should parse ends with operator`` () =
    let result = Filter.create "message $ 'successfully'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", EndOperator, LiteralExpression(StringLiteral "successfully"))) ->
        ()
    | _ -> Assert.Fail("Expected ends with binary expression: message $ 'successfully'")

[<Fact>]
let ``parseExpression should parse AND operator`` () =
    let result = Filter.create "level = 'INFO' & message ~ 'service'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "INFO")),
                            AndOperator,
                            BinaryExpression(MemberExpression "message",
                                             ContainOperator,
                                             LiteralExpression(StringLiteral "service")))) -> ()
    | _ -> Assert.Fail("Expected AND binary expression: level = 'INFO' & message ~ 'service'")

[<Fact>]
let ``parseExpression should parse OR operator`` () =
    let result = Filter.create "level = 'ERROR' | level = 'WARN'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")),
                            OrOperator,
                            BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "WARN")))) -> ()
    | _ -> Assert.Fail("Expected OR binary expression: level = 'ERROR' | level = 'WARN'")

[<Fact>]
let ``parseExpression should handle parentheses for precedence override`` () =
    let result =
        Filter.create "(level = 'ERROR' | level = 'WARN') & message ~ 'database'"

    match result with
    | Some(BinaryExpression(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "ERROR")),
                                             OrOperator,
                                             BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "WARN"))),
                            AndOperator,
                            BinaryExpression(MemberExpression "message",
                                             ContainOperator,
                                             LiteralExpression(StringLiteral "database")))) -> ()
    | _ ->
        Assert.Fail(
            "Expected complex expression with parentheses: (level = 'ERROR' | level = 'WARN') & message ~ 'database'"
        )

[<Fact>]
let ``parseExpression should respect operator precedence without parentheses`` () =
    let result = Filter.create "level = 'INFO' & message ~ 'service' | level = 'ERROR'"

    match result with
    | Some(BinaryExpression(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "INFO")),
                                             AndOperator,
                                             BinaryExpression(MemberExpression "message",
                                                              ContainOperator,
                                                              LiteralExpression(StringLiteral "service"))),
                            OrOperator,
                            BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")))) -> ()
    | _ ->
        Assert.Fail(
            "Expected expression with correct precedence: level = 'INFO' & message ~ 'service' | level = 'ERROR'"
        )

[<Fact>]
let ``parseExpression should parse README example - filter by log level`` () =
    let result = Filter.create "level = 'ERROR'"

    match result with
    | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) -> ()
    | _ -> Assert.Fail("Expected README example: level = 'ERROR'")

[<Fact>]
let ``parseExpression should parse README example - filter by timestamp`` () =
    let result = Filter.create "timestamp > '2025-06-16T21:14:04Z'"

    match result with
    | Some(BinaryExpression(MemberExpression "timestamp",
                            GreaterOperator,
                            LiteralExpression(StringLiteral "2025-06-16T21:14:04Z"))) -> ()
    | _ -> Assert.Fail("Expected README example: timestamp > '2025-06-16T21:14:04Z'")

[<Fact>]
let ``parseExpression should parse README example - search message content`` () =
    let result = Filter.create "message ~ 'database'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", ContainOperator, LiteralExpression(StringLiteral "database"))) ->
        ()
    | _ -> Assert.Fail("Expected README example: message ~ 'database'")

[<Fact>]
let ``parseExpression should parse README example - message starts with`` () =
    let result = Filter.create "message ^ 'Starting'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", StartOperator, LiteralExpression(StringLiteral "Starting"))) ->
        ()
    | _ -> Assert.Fail("Expected README example: message ^ 'Starting'")

[<Fact>]
let ``parseExpression should parse README example - message ends with`` () =
    let result = Filter.create "message $ 'successfully'"

    match result with
    | Some(BinaryExpression(MemberExpression "message", EndOperator, LiteralExpression(StringLiteral "successfully"))) ->
        ()
    | _ -> Assert.Fail("Expected README example: message $ 'successfully'")

[<Fact>]
let ``parseExpression should parse README example - OR condition`` () =
    let result = Filter.create "level = 'ERROR' | level = 'WARN'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")),
                            OrOperator,
                            BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "WARN")))) -> ()
    | _ -> Assert.Fail("Expected README example: level = 'ERROR' | level = 'WARN'")

[<Fact>]
let ``parseExpression should parse README example - AND condition`` () =
    let result = Filter.create "level = 'INFO' & message ~ 'service'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "INFO")),
                            AndOperator,
                            BinaryExpression(MemberExpression "message",
                                             ContainOperator,
                                             LiteralExpression(StringLiteral "service")))) -> ()
    | _ -> Assert.Fail("Expected README example: level = 'INFO' & message ~ 'service'")

[<Fact>]
let ``parseExpression should parse README example - recent errors`` () =
    let result = Filter.create "level = 'ERROR' & timestamp > '2025-06-16T21:14:04Z'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")),
                            AndOperator,
                            BinaryExpression(MemberExpression "timestamp",
                                             GreaterOperator,
                                             LiteralExpression(StringLiteral "2025-06-16T21:14:04Z")))) -> ()
    | _ -> Assert.Fail("Expected README example: level = 'ERROR' & timestamp > '2025-06-16T21:14:04Z'")

[<Fact>]
let ``parseExpression should parse README example - complex filtering`` () =
    let result =
        Filter.create "(level = 'ERROR' | level = 'WARN') & message ~ 'database'"

    match result with
    | Some(BinaryExpression(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "ERROR")),
                                             OrOperator,
                                             BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "WARN"))),
                            AndOperator,
                            BinaryExpression(MemberExpression "message",
                                             ContainOperator,
                                             LiteralExpression(StringLiteral "database")))) -> ()
    | _ -> Assert.Fail("Expected README example: (level = 'ERROR' | level = 'WARN') & message ~ 'database'")

[<Fact>]
let ``parseExpression should handle whitespace correctly`` () =
    let result1 = Filter.create "level='ERROR'"
    let result2 = Filter.create " level = 'ERROR' "
    let result3 = Filter.create "level   =   'ERROR'"

    match result1, result2, result3 with
    | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))),
      Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))),
      Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) -> ()
    | _ -> Assert.Fail("Expected all whitespace variations to parse correctly")

[<Fact>]
let ``parseExpression should return None for invalid syntax`` () =
    let invalidExpressions =
        [ "level = = 'ERROR'"
          "level =="
          "& level = 'ERROR'"
          "level = 'ERROR' &"
          "level = 'ERROR"
          "'ERROR' = level ="
          "(level = 'ERROR'"
          "level = 'ERROR')"
          "level ~ ~ 'test'"
          ""
          "   " ]

    for expr in invalidExpressions do
        let result = Filter.create expr
        Assert.True(result.IsNone, $"Expected None for invalid expression: '{expr}'")

[<Fact>]
let ``parseExpression should handle member names with underscores`` () =
    let result = Filter.create "log_level = 'ERROR'"

    match result with
    | Some(BinaryExpression(MemberExpression "log_level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) ->
        ()
    | _ -> Assert.Fail("Expected to parse member names with underscores")

[<Fact>]
let ``parseExpression should handle empty strings`` () =
    let result = Filter.create "message = ''"

    match result with
    | Some(BinaryExpression(MemberExpression "message", EqualOperator, LiteralExpression(StringLiteral ""))) -> ()
    | _ -> Assert.Fail("Expected to parse empty string literals")

[<Fact>]
let ``parseExpression should handle nested parentheses`` () =
    let result = Filter.create "((level = 'ERROR'))"

    match result with
    | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) -> ()
    | _ -> Assert.Fail("Expected to parse nested parentheses")

[<Fact>]
let ``parseExpression should handle complex precedence scenarios`` () =
    // Test: A | B & C should parse as A | (B & C)
    let result = Filter.create "a = '1' | b = '2' & c = '3'"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "a", EqualOperator, LiteralExpression(StringLiteral "1")),
                            OrOperator,
                            BinaryExpression(BinaryExpression(MemberExpression "b",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "2")),
                                             AndOperator,
                                             BinaryExpression(MemberExpression "c",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "3"))))) -> ()
    | _ ->
        Assert.Fail(
            "Expected correct precedence: a = '1' | b = '2' & c = '3' should parse as a = '1' | (b = '2' & c = '3')"
        )

[<Fact>]
let ``parseExpression should parse NOT operator with parentheses (no space)`` () =
    let result = Filter.create "!(level = 'ERROR')"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")),
                            NotOperator,
                            LiteralExpression(StringLiteral ""))) -> ()
    | _ -> Assert.Fail("Expected NOT expression: !(level = 'ERROR')")

[<Fact>]
let ``parseExpression should parse NOT operator with parentheses (space)`` () =
    let result = Filter.create "! (level = 'ERROR')"

    match result with
    | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "ERROR")),
                            NotOperator,
                            LiteralExpression(StringLiteral ""))) -> ()
    | _ -> Assert.Fail("Expected NOT expression: ! (level = 'ERROR')")

[<Fact>]
let ``parseExpression should parse NOT with OR precedence`` () =
    let result = Filter.create "!(level = 'ERROR') | level = 'WARN'"

    match result with
    | Some(BinaryExpression(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                              EqualOperator,
                                                              LiteralExpression(StringLiteral "ERROR")),
                                             NotOperator,
                                             LiteralExpression(StringLiteral "")),
                            OrOperator,
                            BinaryExpression(MemberExpression "level",
                                             EqualOperator,
                                             LiteralExpression(StringLiteral "WARN")))) -> ()
    | _ -> Assert.Fail("Expected NOT precedence with OR: !(level = 'ERROR') | level = 'WARN'")

[<Fact>]
let ``parseExpression should treat double NOT as invalid`` () =
    let result = Filter.create "!!(level = 'ERROR')"
    Assert.True(result.IsNone, "Expected double NOT to be invalid until supported")

[<Fact>]
let ``parseExpression should treat binary ! operator as invalid`` () =
    let result = Filter.create "level ! 'ERROR'"
    Assert.True(result.IsNone, "Expected binary ! to be invalid")

[<Fact>]
let ``parseExpression should treat number followed by identifier as invalid member`` () =
    let result = Filter.create "123abc"
    Assert.True(result.IsNone, "Expected 123abc to be invalid (number literal cannot be followed by identifier)")

[<Fact>]
let ``parseExpression should parse member names with digits`` () =
    let result = Filter.create "version1 = '1.0'"

    match result with
    | Some(BinaryExpression(MemberExpression "version1", EqualOperator, LiteralExpression(StringLiteral "1.0"))) -> ()
    | _ -> Assert.Fail("Expected to parse member names containing digits: version1")
