namespace Analog.Cli

open System.Text.RegularExpressions

type Template =
    { Name: string
      Regex: Regex
      Dimensions: Dimension list }

    static Default =
        { Name = nameof Template.Default
          Regex =
            Regex(
                "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
                RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
            )
          Dimensions =
            [ { Name = "Timestamp"
                Type = DimensionType.Timestamp }
              { Name = "Severity"
                Type = DimensionType.String }
              { Name = "Message"
                Type = DimensionType.String } ] }

    static Quasar =
        { Name = nameof Template.Quasar
          Regex =
            Regex(
                "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{8})?  (?<Scope>[A-z0-9]+)? (?<RequestId>[A-Z0-9]{13}:[0-9]{8})? (?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
                RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
            )
          Dimensions = List.empty }
