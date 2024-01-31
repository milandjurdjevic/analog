module Analog.Cli.Unchecked

let isNotDefault (value: 'a) = value <> Unchecked.defaultof<'a>
