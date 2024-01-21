module Analog.Console.Value

let notDefault value = value <> Unchecked.defaultof<'a>

let hasType<'a> value = value.GetType() = typeof<'a>
