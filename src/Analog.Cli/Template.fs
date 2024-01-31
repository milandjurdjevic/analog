module Analog.Cli.Template

open System.Text.RegularExpressions

let basic =
    Regex(
        "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
        RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
    )
