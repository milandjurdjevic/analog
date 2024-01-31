module Analog.Cli.Option

let ofBooleanValue (ok: bool, value) = if ok then Some value else None
