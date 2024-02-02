namespace Analog.Cli

open System.Text.RegularExpressions

type Template =
    { Name: string
      Regex: Regex }

    static Default =
        { Name = nameof Template.Default
          Regex =
            Regex(
                "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
                RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
            ) }
