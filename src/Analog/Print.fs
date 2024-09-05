module Analog.Print

open Spectre.Console
open Spectre.Console.Json
open System.Text.Json

let json: obj -> unit =
    JsonSerializer.Serialize
    >> JsonText
    >> AnsiConsole.Write
    >> AnsiConsole.WriteLine

let error message =
    Text(message, Style(foreground = Color.Red)) |> AnsiConsole.Write
