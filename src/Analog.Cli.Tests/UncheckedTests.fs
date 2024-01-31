module Analog.Cli.Tests.UncheckedTests

open System
open Analog.Cli
open Xunit

[<Fact>]
let isNotDefault_valueIsDefault_true () =
    Unchecked.defaultof<string> |> Unchecked.isNotDefault |> Assert.False

[<Fact>]
let isNotDefault_valueNotDefault_true () =
    String.Empty |> Unchecked.isNotDefault |> Assert.True
