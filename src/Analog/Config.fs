module Analog.Config

open System
open System.IO
open System.Collections.Generic
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

[<CLIMutable>]
type Configuration = { Pattern: string }

let private deserializer =
    DeserializerBuilder()
    |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
    |> _.Build()

let templates =
    deserializer.Deserialize<IDictionary<string, Configuration>>(File.ReadAllText("template.yml"))
    |> Seq.map (fun template -> template.Key, template.Value)
    |> Map.ofSeq

type Pattern = | Regex of System.Text.RegularExpressions.Regex

type PropertyType =
    | String of string
    | Integer of int
    | Float of float
    | Boolean of bool
    | Timestamp of DateTimeOffset

type Property = string * PropertyType

type Template =
    | Property of Property
    | Separator of string