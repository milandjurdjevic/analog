namespace Analog.Cli

open System

type Severity =
    | Trace = 0
    | Debug = 1
    | Info = 2
    | Warning = 3
    | Error = 4
    | Critical = 5

module SeverityParser =
    let ofString (value: string) =
        match value with
        | trace when trace.Equals("TRACE", StringComparison.InvariantCultureIgnoreCase) -> Severity.Trace
        | debug when debug.Equals("DEBUG", StringComparison.InvariantCultureIgnoreCase) -> Severity.Debug
        | debug when debug.Equals("DBG", StringComparison.InvariantCultureIgnoreCase) -> Severity.Debug
        | info when info.Equals("INFO", StringComparison.InvariantCultureIgnoreCase) -> Severity.Info
        | info when info.Equals("INF", StringComparison.InvariantCultureIgnoreCase) -> Severity.Info
        | warning when warning.Equals("WRN", StringComparison.InvariantCultureIgnoreCase) -> Severity.Warning
        | warning when warning.Equals("WARN", StringComparison.InvariantCultureIgnoreCase) -> Severity.Warning
        | warning when warning.Equals("WARNING", StringComparison.InvariantCultureIgnoreCase) -> Severity.Warning
        | error when error.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase) -> Severity.Error
        | error when error.Equals("ERR", StringComparison.InvariantCultureIgnoreCase) -> Severity.Error
        | critical when critical.Equals("CRITICAL", StringComparison.InvariantCultureIgnoreCase) -> Severity.Critical
        | critical when critical.Equals("FATAL", StringComparison.InvariantCultureIgnoreCase) -> Severity.Critical
        | _ -> Severity.Trace
