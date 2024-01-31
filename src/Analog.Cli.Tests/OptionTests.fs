module Analog.Cli.Tests.OptionTests

open Analog.Cli
open System
open Xunit

[<Fact>]
let ofBooleanValue_okIsTrue_some () =
    (true, String.Empty) |> Option.ofBooleanValue |> Option.isSome |> Assert.True

[<Fact>]
let ofBooleanValue_okIsFalse_none () =
    (false, String.Empty) |> Option.ofBooleanValue |> Option.isNone |> Assert.True
