module Analog.Cli.Tests.ParsedTests

open Analog.Cli
open System
open Xunit

[<Fact>]
let toOption_okIsTrue_some () =
    (true, String.Empty) |> Parsed.toOption |> Option.isSome |> Assert.True
    
[<Fact>]
let toOption_okIsFalse_none () =
    (false, String.Empty) |> Parsed.toOption |> Option.isNone |> Assert.True
