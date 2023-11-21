namespace EducationalCode

module OptionModule =

    let map f option =
        match option with
        | Some value -> Some (f value)
        | None       -> None

    let bind f option =
        match option with
        | Some value -> f value
        | None       -> None

module ResultModule =

    let map f result =
        match result with
        | Ok value    -> Ok (f value)
        | Error error -> Error error

    let bind f result =
        match result with
        | Ok value    -> f value
        | Error error -> Error error

