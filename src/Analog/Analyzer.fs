namespace Analog


module Analyzer =
    let CountSeverity (logs: Log seq) =
        logs |> Seq.countBy (_.Severity) |> readOnlyDict
