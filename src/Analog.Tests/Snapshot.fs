module Analog.Tests.Snapshot

open System
open System.Security.Cryptography
open System.Text
open VerifyTests
open VerifyXunit

type Opt =
    { Sub: string option
      Arg: string option
      Data: obj }

let cmp (data: obj) =
    { Data = data
      Arg = None
      Sub = None }

let sub (identifier: Guid) (options: Opt) =
    { options with
        Sub = Some(identifier.ToString "n") }

let arg (arguments: obj list) (options: Opt) =
    use hash = SHA256.Create()

    { options with
        Arg =
            (String.Empty, arguments)
            ||> List.fold (fun acc arg -> acc + arg.ToString())
            |> Encoding.UTF8.GetBytes
            |> hash.ComputeHash
            |> BitConverter.ToString
            |> _.Replace("-", "").ToLower()
            |> Some }

let run (options: Opt) =
    let verify = VerifySettings()
    verify.UseDirectory "snap"

    let file =
        match options with
        | { Sub = Some name
            Arg = Some args
            Data = _ } -> Some $"{name}_{args}"
        | { Sub = Some name
            Arg = None
            Data = _ } -> Some name
        | _ -> None

    if file.IsSome then
        verify.UseFileName(file.Value)

    Verifier.Verify(options.Data, verify).ToTask()
