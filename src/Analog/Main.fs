open System.Text
open System.Text.Json
open Analog

open System.IO

[<EntryPoint>]
let main args =
    match Args.parse args with
    | Ok args ->
        let output content =
            use stream =
                Args.output args
                |> Option.defaultValue (Path.Combine(Directory.GetCurrentDirectory(), "analog.json"))
                |> File.Create

            content |> JsonSerializer.Serialize |> Encoding.UTF8.GetBytes |> stream.Write

        let input = Args.input args |> List.filter File.Exists |> List.map File.ReadAllText

        let pattern =
            Args.pattern args
            |> Option.map LogPattern.create
            |> Option.defaultValue LogPattern.standard

        let parse =
            List.map (fun text -> pattern |> LogPattern.parse text)
            >> List.fold (fun acc next -> acc @ next) List.empty
            >> List.map LogEntry.box

        input |> parse |> output
        0
    | Error error ->
        error |> eprintfn "%s"
        1
