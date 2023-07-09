namespace DiscriminatedUnions 

type ResultSW<'TSuccess,'TFailure> =
    | Success of 'TSuccess
    | Failure of 'TFailure

[<Struct>]
type Validity =
    | CurrentValidity 
    | FutureValidity 
    | ReplacementService 
    | WithoutReplacementService 