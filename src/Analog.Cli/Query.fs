module Analog.Cli.Query

open System
open System.Linq
open System.Linq.Dynamic.Core

let filter (expression: string) (source: IQueryable<'T>) =
    if String.IsNullOrWhiteSpace(expression) then
        source
    else
        source.Where(expression)
