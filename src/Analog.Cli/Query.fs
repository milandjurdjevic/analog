module Analog.Cli.Query

open System
open System.Linq
open System.Linq.Dynamic.Core

let private expressionToOption (expression: string) =
    if String.IsNullOrWhiteSpace(expression) then
        None
    else
        Some expression

let filter (expression: string) (source: IQueryable<'a>) =
    expressionToOption expression
    |> Option.map source.Where
    |> Option.defaultValue source

let sort (expression: string) (source: IQueryable<'a>) =
    expressionToOption expression
    |> Option.map source.OrderBy
    |> Option.map (fun it -> it :> IQueryable<'a>)
    |> Option.defaultValue source
