open System.Text
open System.Text.Json
open System.IO

open Analog.Filters
open Analog.Logs
open Analog.Args

let filter args =
    getFilterArg args
    |> Option.bind createExpression
    |> Option.map (evalExpression >> List.filter)
    |> Option.defaultValue id

let parse args =
    let pattern =
        getPatternArg args
        |> Option.map createPattern
        |> Option.defaultValue defaultPattern

    List.map (fun text -> pattern |> evalPattern text)
    >> List.fold List.append List.empty

let input args =
    getInputArgs args |> List.filter File.Exists |> List.map File.ReadAllText

let output args content =
    use stream =
        getOutputArg args
        |> Option.defaultValue (Path.Combine(Directory.GetCurrentDirectory(), "analog.json"))
        |> File.Create

    content |> JsonSerializer.Serialize |> Encoding.UTF8.GetBytes |> stream.Write

[<EntryPoint>]
let main args =
    match parseArg args with
    | Ok args ->
        input args |> parse args |> filter args |> List.map boxEntry |> output args
        0
    | Error error ->
        error |> eprintfn "%s"
        1
