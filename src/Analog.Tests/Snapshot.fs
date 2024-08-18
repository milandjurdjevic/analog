module Analog.Tests.Snapshot

open VerifyTests
open VerifyXunit

let compare (data: obj) =
    let settings = VerifySettings()
    settings.UseDirectory "snap"
    Verifier.Verify(data, settings)

let input (param: obj array) (task: SettingsTask) =
    task.UseParameters(param).HashParameters()

let exec (task: SettingsTask) = task.ToTask()
