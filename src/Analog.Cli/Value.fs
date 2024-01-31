module Analog.Cli.Value

let notDefault value = value <> Unchecked.defaultof<'a>

let hasType<'a> value = value.GetType() = typeof<'a>

let toOption (ok: bool, value) = if ok then Some value else None
