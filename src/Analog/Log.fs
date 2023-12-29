namespace Analog

open System

type Severity =
    | Trace = 1
    | Debug = 2
    | Information = 3
    | Warning = 4
    | Error = 5
    | Critical = 6

type Log =
    { Severity: Severity
      Timestamp: DateTimeOffset
      Message: string }
