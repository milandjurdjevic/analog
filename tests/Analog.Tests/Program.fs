open Expecto

open Analog.Tests.FilterEvaluationTests
open Analog.Tests.FilterParserTests
open Analog.Tests.AdditionalTests

[<EntryPoint>]
let main argv =
    let all =
        testList "All tests" [
            Analog.Tests.FilterEvaluationTests.tests
            Analog.Tests.FilterParserTests.tests
            Analog.Tests.AdditionalTests.tests
        ]
    runTestsWithCLIArgs [] argv all
