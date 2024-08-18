module Analog.Template

open System.IO
open System.Collections.Generic
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

[<CLIMutable>]
type Configuration = { Pattern: string }

let configuration =
    DeserializerBuilder()
    |> _.WithNamingConvention(CamelCaseNamingConvention.Instance)
    |> _.Build()
    |> _.Deserialize<IDictionary<string, Configuration>>(File.ReadAllText("template.yml"))
    |> Seq.map (fun template -> template.Key, template.Value)
    |> Map.ofSeq
