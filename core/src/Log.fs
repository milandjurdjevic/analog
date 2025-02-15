module Analog.Core.Log

open System

type Literal =
    | StringLiteral of string
    | NumberLiteral of float
    | BooleanLiteral of bool
    | TimestampLiteral of DateTimeOffset

type Entry = Entry of Map<string, Literal>

let literal key (Entry entry) = entry |> Map.tryFind key

let empty = Map.empty |> Entry
