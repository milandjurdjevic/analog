namespace Analog.Cli

open System.Text.RegularExpressions
open Spectre.Console

type Template =
    { Name: string
      Regex: Regex
      Highlighting: Map<string, Map<string, Color>> }

    static Default =
        { Name = nameof Template.Default
          Regex =
            Regex(
                "^\[(?<Timestamp>[\d\-]{10} [\d\:\.\+\ ]{19})?\] \[(?<Severity>[A-Z]{3})?\] (?<Message>[\s\S]*?\n*(?=^\[[\d\-]{10}.*?(?:[^ \n]+ )|\z))",
                RegexOptions.Multiline ||| RegexOptions.Compiled ||| RegexOptions.IgnoreCase
            )
          Highlighting =
            Map
                [ "Severity",
                  Map
                      [ "ERR", Color.Red3
                        "CRT", Color.Red1
                        "FTL", Color.Red1
                        "WRN", Color.Orange1
                        "INF", Color.SpringGreen3
                        "DBG", Color.DeepSkyBlue1
                        "TRC", Color.SteelBlue1 ] ] }
