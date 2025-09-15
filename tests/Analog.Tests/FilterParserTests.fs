module Analog.Tests.FilterParserTests

open Expecto
open Analog.Logs
open Analog.Filters

let literalParsingTests =
    testCase "parseExpression should parse string literals"
    <| fun _ ->
        match tryCreateFilterExpression "'hello'" with
        | Some(LiteralExpression(StringLiteral "hello")) -> ()
        | other -> failtestf "Expected string literal 'hello' but got %A" other

let numberParsingTests =
    testCase "parseExpression should parse number literals"
    <| fun _ ->
        match tryCreateFilterExpression "42.5" with
        | Some(LiteralExpression(NumberLiteral 42.5)) -> ()
        | other -> failtestf "Expected number literal 42.5 but got %A" other

let booleanParsingTests =
    testCase "parseExpression should parse boolean literals"
    <| fun _ ->
        match tryCreateFilterExpression "true" with
        | Some(LiteralExpression(BooleanLiteral true)) -> ()
        | other -> failtestf "Expected boolean literal true got %A" other

        match tryCreateFilterExpression "false" with
        | Some(LiteralExpression(BooleanLiteral false)) -> ()
        | other -> failtestf "Expected boolean literal false got %A" other

let memberParsingTests =
    testCase "parseExpression should parse member expressions"
    <| fun _ ->
        match tryCreateFilterExpression "level" with
        | Some(MemberExpression "level") -> ()
        | other -> failtestf "Expected member expression 'level' got %A" other

        match tryCreateFilterExpression "message" with
        | Some(MemberExpression "message") -> ()
        | other -> failtestf "Expected member expression 'message' got %A" other

let operatorParsingTests =
    testList
        "Operator parsing"
        [ testCase "parseExpression should parse equal operator"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'ERROR'" with
              | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) ->
                  ()
              | other -> failtestf "Expected equal expression got %A" other
          testCase "parseExpression should parse greater than operator"
          <| fun _ ->
              match tryCreateFilterExpression "count > 10" with
              | Some(BinaryExpression(MemberExpression "count", GreaterOperator, LiteralExpression(NumberLiteral 10.0))) ->
                  ()
              | other -> failtestf "Expected greater than expression got %A" other
          testCase "parseExpression should parse less than operator"
          <| fun _ ->
              match tryCreateFilterExpression "count < 5" with
              | Some(BinaryExpression(MemberExpression "count", LessOperator, LiteralExpression(NumberLiteral 5.0))) ->
                  ()
              | other -> failtestf "Expected less than expression got %A" other
          testCase "parseExpression should parse contains operator"
          <| fun _ ->
              match tryCreateFilterExpression "message ~ 'database'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      ContainOperator,
                                      LiteralExpression(StringLiteral "database"))) -> ()
              | other -> failtestf "Expected contains expression got %A" other
          testCase "parseExpression should parse starts with operator"
          <| fun _ ->
              match tryCreateFilterExpression "message ^ 'Starting'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      StartOperator,
                                      LiteralExpression(StringLiteral "Starting"))) -> ()
              | other -> failtestf "Expected starts expression got %A" other
          testCase "parseExpression should parse ends with operator"
          <| fun _ ->
              match tryCreateFilterExpression "message $ 'successfully'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      EndOperator,
                                      LiteralExpression(StringLiteral "successfully"))) -> ()
              | other -> failtestf "Expected ends expression got %A" other ]

let logicalOperatorParsingTests =
    testList
        "Logical operators"
        [ testCase "parseExpression should parse AND operator"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'INFO' & message ~ 'service'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "INFO")),
                                      AndOperator,
                                      BinaryExpression(MemberExpression "message",
                                                       ContainOperator,
                                                       LiteralExpression(StringLiteral "service")))) -> ()
              | other -> failtestf "Expected AND expression got %A" other
          testCase "parseExpression should parse OR operator"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'ERROR' | level = 'WARN'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "ERROR")),
                                      OrOperator,
                                      BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "WARN")))) -> ()
              | other -> failtestf "Expected OR expression got %A" other
          testCase "parseExpression should handle parentheses for precedence override"
          <| fun _ ->
              match tryCreateFilterExpression "(level = 'ERROR' | level = 'WARN') & message ~ 'database'" with
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
              | other -> failtestf "Expected complex parentheses expression got %A" other
          testCase "parseExpression should respect operator precedence without parentheses"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'INFO' & message ~ 'service' | level = 'ERROR'" with
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
              | other -> failtestf "Expected precedence expression got %A" other ]

let readmeExampleTests =
    testList
        "README examples"
        [ testCase "filter by log level"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'ERROR'" with
              | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) ->
                  ()
              | other -> failtestf "Expected level = 'ERROR' got %A" other
          testCase "filter by timestamp"
          <| fun _ ->
              match tryCreateFilterExpression "timestamp > '2025-06-16T21:14:04Z'" with
              | Some(BinaryExpression(MemberExpression "timestamp",
                                      GreaterOperator,
                                      LiteralExpression(StringLiteral "2025-06-16T21:14:04Z"))) -> ()
              | other -> failtestf "Expected timestamp > ... got %A" other
          testCase "search message content"
          <| fun _ ->
              match tryCreateFilterExpression "message ~ 'database'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      ContainOperator,
                                      LiteralExpression(StringLiteral "database"))) -> ()
              | other -> failtestf "Expected message ~ 'database' got %A" other
          testCase "message starts with"
          <| fun _ ->
              match tryCreateFilterExpression "message ^ 'Starting'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      StartOperator,
                                      LiteralExpression(StringLiteral "Starting"))) -> ()
              | other -> failtestf "Expected message ^ 'Starting' got %A" other
          testCase "message ends with"
          <| fun _ ->
              match tryCreateFilterExpression "message $ 'successfully'" with
              | Some(BinaryExpression(MemberExpression "message",
                                      EndOperator,
                                      LiteralExpression(StringLiteral "successfully"))) -> ()
              | other -> failtestf "Expected message $ 'successfully' got %A" other
          testCase "OR condition"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'ERROR' | level = 'WARN'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "ERROR")),
                                      OrOperator,
                                      BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "WARN")))) -> ()
              | other -> failtestf "Expected OR condition got %A" other
          testCase "AND condition"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'INFO' & message ~ 'service'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "INFO")),
                                      AndOperator,
                                      BinaryExpression(MemberExpression "message",
                                                       ContainOperator,
                                                       LiteralExpression(StringLiteral "service")))) -> ()
              | other -> failtestf "Expected AND condition got %A" other
          testCase "recent errors"
          <| fun _ ->
              match tryCreateFilterExpression "level = 'ERROR' & timestamp > '2025-06-16T21:14:04Z'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "ERROR")),
                                      AndOperator,
                                      BinaryExpression(MemberExpression "timestamp",
                                                       GreaterOperator,
                                                       LiteralExpression(StringLiteral "2025-06-16T21:14:04Z")))) -> ()
              | other -> failtestf "Expected recent errors condition got %A" other
          testCase "complex filtering"
          <| fun _ ->
              match tryCreateFilterExpression "(level = 'ERROR' | level = 'WARN') & message ~ 'database'" with
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
              | other -> failtestf "Expected complex filtering got %A" other ]


let whitespaceTests =
    testCase "parseExpression should handle whitespace correctly"
    <| fun _ ->
        let r1 = tryCreateFilterExpression "level='ERROR'"
        let r2 = tryCreateFilterExpression " level = 'ERROR' "
        let r3 = tryCreateFilterExpression "level   =   'ERROR'"

        match r1, r2, r3 with
        | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))),
          Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))),
          Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) ->
            ()
        | _ -> failtest "Whitespace variations failed"

let invalidSyntaxTests =
    testCase "parseExpression should return None for invalid syntax"
    <| fun _ ->
        let invalids =
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

        invalids
        |> List.iter (fun expr ->
            match tryCreateFilterExpression expr with
            | None -> ()
            | other -> failtestf "Expected None for invalid expression %s but got %A" expr other)

let memberNameVariationTests =
    testList
        "Member naming variations"
        [ testCase "parseExpression should handle member names with underscores"
          <| fun _ ->
              match tryCreateFilterExpression "log_level = 'ERROR'" with
              | Some(BinaryExpression(MemberExpression "log_level",
                                      EqualOperator,
                                      LiteralExpression(StringLiteral "ERROR"))) -> ()
              | other -> failtestf "Expected log_level member got %A" other
          testCase "parseExpression should handle empty strings"
          <| fun _ ->
              match tryCreateFilterExpression "message = ''" with
              | Some(BinaryExpression(MemberExpression "message", EqualOperator, LiteralExpression(StringLiteral ""))) ->
                  ()
              | other -> failtestf "Expected empty string literal got %A" other
          testCase "parseExpression should handle nested parentheses"
          <| fun _ ->
              match tryCreateFilterExpression "((level = 'ERROR'))" with
              | Some(BinaryExpression(MemberExpression "level", EqualOperator, LiteralExpression(StringLiteral "ERROR"))) ->
                  ()
              | other -> failtestf "Expected nested parentheses got %A" other
          testCase "parseExpression should handle complex precedence scenarios"
          <| fun _ ->
              match tryCreateFilterExpression "a = '1' | b = '2' & c = '3'" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "a",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "1")),
                                      OrOperator,
                                      BinaryExpression(BinaryExpression(MemberExpression "b",
                                                                        EqualOperator,
                                                                        LiteralExpression(StringLiteral "2")),
                                                       AndOperator,
                                                       BinaryExpression(MemberExpression "c",
                                                                        EqualOperator,
                                                                        LiteralExpression(StringLiteral "3"))))) -> ()
              | other -> failtestf "Expected complex precedence scenario got %A" other
          testCase "parseExpression should parse NOT operator with parentheses (no space)"
          <| fun _ ->
              match tryCreateFilterExpression "!(level = 'ERROR')" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "ERROR")),
                                      NotOperator,
                                      LiteralExpression(StringLiteral ""))) -> ()
              | other -> failtestf "Expected NOT (no space) got %A" other
          testCase "parseExpression should parse NOT operator with parentheses (space)"
          <| fun _ ->
              match tryCreateFilterExpression "! (level = 'ERROR')" with
              | Some(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "ERROR")),
                                      NotOperator,
                                      LiteralExpression(StringLiteral ""))) -> ()
              | other -> failtestf "Expected NOT (space) got %A" other
          testCase "parseExpression should parse NOT with OR precedence"
          <| fun _ ->
              match tryCreateFilterExpression "!(level = 'ERROR') | level = 'WARN'" with
              | Some(BinaryExpression(BinaryExpression(BinaryExpression(MemberExpression "level",
                                                                        EqualOperator,
                                                                        LiteralExpression(StringLiteral "ERROR")),
                                                       NotOperator,
                                                       LiteralExpression(StringLiteral "")),
                                      OrOperator,
                                      BinaryExpression(MemberExpression "level",
                                                       EqualOperator,
                                                       LiteralExpression(StringLiteral "WARN")))) -> ()
              | other -> failtestf "Expected NOT precedence with OR got %A" other
          testCase "parseExpression should treat double NOT as invalid"
          <| fun _ ->
              match tryCreateFilterExpression "!!(level = 'ERROR')" with
              | None -> ()
              | other -> failtestf "Expected double NOT invalid but got %A" other
          testCase "parseExpression should treat binary ! operator as invalid"
          <| fun _ ->
              match tryCreateFilterExpression "level ! 'ERROR'" with
              | None -> ()
              | other -> failtestf "Expected binary ! invalid but got %A" other
          testCase "parseExpression should treat number followed by identifier as invalid member"
          <| fun _ ->
              match tryCreateFilterExpression "123abc" with
              | None -> ()
              | other -> failtestf "Expected 123abc invalid but got %A" other
          testCase "parseExpression should parse member names with digits"
          <| fun _ ->
              match tryCreateFilterExpression "version1 = '1.0'" with
              | Some(BinaryExpression(MemberExpression "version1", EqualOperator, LiteralExpression(StringLiteral "1.0"))) ->
                  ()
              | other -> failtestf "Expected version1 member got %A" other ]

let allParserTests =
    testList
        "Filter parser"
        [ literalParsingTests
          numberParsingTests
          booleanParsingTests
          memberParsingTests
          operatorParsingTests
          logicalOperatorParsingTests
          readmeExampleTests
          whitespaceTests
          invalidSyntaxTests
          memberNameVariationTests ]

// Expose list for aggregation
let tests = allParserTests
