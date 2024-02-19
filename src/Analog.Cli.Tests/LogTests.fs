[<VerifyXunit.UsesVerify>]
module Analog.Cli.Tests.LogTests

open System.IO
open System.Threading
open Analog.Cli
open FSharp.Control
open VerifyXunit
open Xunit

[<Theory>]
[<InlineData("testlog_default.log")>]
let ``Parse log stream from file`` (file: string) =
    let directory = Directory.GetCurrentDirectory()
    let path = Path.Combine [| directory; file |]
    use stream = File.OpenRead path
    let template = Template.Init() |> Seq.head

    let logs = stream |> Log.ofStream template CancellationToken.None |> TaskSeq.toSeq

    Verifier.Verify logs
    |> fun settings -> settings.UseParameters file |> _.HashParameters() |> _.ToTask()
    |> Async.AwaitTask
