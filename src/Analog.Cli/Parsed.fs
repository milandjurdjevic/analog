module Analog.Cli.Parsed

let toOption (ok: bool, value) = if ok then Some value else None
