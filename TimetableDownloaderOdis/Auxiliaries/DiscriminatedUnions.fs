namespace DiscriminatedUnions 

type internal ResultSW<'TSuccess,'TFailure> =
    | Success of 'TSuccess
    | Failure of 'TFailure

[<Struct>]
type internal Validity =
    | CurrentValidity 
    | FutureValidity 
    | ReplacementService 
    | WithoutReplacementService 