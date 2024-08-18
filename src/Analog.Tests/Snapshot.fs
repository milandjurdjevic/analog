module Analog.Tests.Snapshot

open System.Threading.Tasks
open VerifyTests
open VerifyXunit

let compare (data: obj) : Task =
    let settings = VerifySettings()
    settings.UseDirectory "snap"
    Verifier.Verify(data, settings).ToTask()
