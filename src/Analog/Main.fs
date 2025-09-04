open System
open System.IO
open System.Text
open System.Text.Json

open Analog.Logs
open Analog.Filters
open Analog.Args

match Environment.GetCommandLineArgs() |> Array.skip 1 |> Arg.parse with
| Ok value ->
    let filter =
        value.Filter
        |> Option.bind Filter.create
        |> Option.map Filter.eval
        |> Option.defaultValue (fun _ -> true)

    let extract logs =
        value.Pattern
        |> Option.map Pattern.create
        |> Option.defaultValue Pattern.preset
        |> Pattern.eval logs

    use output =
        value.Output
        |> Option.defaultValue (Path.Combine(Directory.GetCurrentDirectory(), "analog.json"))
        |> File.Create

    value.Inputs
    |> Seq.map (File.ReadAllText >> extract)
    |> Seq.collect (List.filter filter)
    |> Seq.map Entry.toObj
    |> Seq.toArray
    |> JsonSerializer.Serialize
    |> Encoding.UTF8.GetBytes
    |> output.Write

| Error error -> failwith error
