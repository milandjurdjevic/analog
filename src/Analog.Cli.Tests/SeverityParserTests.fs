module Analog.Cli.Tests.SeverityParserTests

open Analog.Cli
open Xunit

[<Theory>]
// Trace
[<InlineData("some-random-value", Severity.Trace)>]
[<InlineData("TRACE", Severity.Trace)>]
[<InlineData("Trace", Severity.Trace)>]
[<InlineData("trace", Severity.Trace)>]
// Debug
[<InlineData("DEBUG", Severity.Debug)>]
[<InlineData("Debug", Severity.Debug)>]
[<InlineData("debug", Severity.Debug)>]
[<InlineData("DBG", Severity.Debug)>]
[<InlineData("dbg", Severity.Debug)>]
[<InlineData("Dbg", Severity.Debug)>]
// Info
[<InlineData("INFO", Severity.Info)>]
[<InlineData("Info", Severity.Info)>]
[<InlineData("info", Severity.Info)>]
// Warning
[<InlineData("WARN", Severity.Warning)>]
[<InlineData("Warn", Severity.Warning)>]
[<InlineData("warn", Severity.Warning)>]
[<InlineData("WARNING", Severity.Warning)>]
[<InlineData("Warning", Severity.Warning)>]
[<InlineData("warning", Severity.Warning)>]
// Error
[<InlineData("ERR", Severity.Error)>]
[<InlineData("Err", Severity.Error)>]
[<InlineData("err", Severity.Error)>]
[<InlineData("ERROR", Severity.Error)>]
[<InlineData("Error", Severity.Error)>]
[<InlineData("error", Severity.Error)>]
// Critical
[<InlineData("FATAL", Severity.Critical)>]
[<InlineData("Fatal", Severity.Critical)>]
[<InlineData("fatal", Severity.Critical)>]
[<InlineData("CRITICAL", Severity.Critical)>]
[<InlineData("Critical", Severity.Critical)>]
[<InlineData("critical", Severity.Critical)>]
let ofString_returnsExpectedTrace (severityString: string) (severityType: Severity) =
    SeverityParser.ofString severityString
    |> (fun parsed -> parsed = severityType)
    |> Assert.True
