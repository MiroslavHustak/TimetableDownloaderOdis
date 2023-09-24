namespace ErrorHandling

open System
open System.IO

open Parsing
open DiscriminatedUnions
            
module Result = 
                   
    let internal toOption f : 'a option = 
        f                      
        |> function   
            | Ok value -> Some value 
            | Error _  -> None  
        
    let internal sequence aListOfResults =

        let prepend firstR restR =
            match firstR, restR with
            | Ok first, Ok rest   -> Ok (first::rest) | Error err1, Ok _ -> Error err1
            | Ok _, Error err2    -> Error err2
            | Error err1, Error _ -> Error err1

        let initialValue = Ok [] 
        List.foldBack prepend aListOfResults initialValue

    let internal sequence1 aListOfResults =            
        aListOfResults 
        |> List.choose (fun item -> item |> Result.toOption)
        |> List.length
        |> function   
            | 0 -> 
                let err = 
                    aListOfResults 
                    |> List.map (fun item ->
                                          match item with
                                          | Ok _      -> String.Empty
                                          | Error err -> err
                                ) |> List.head //One exception or None is enough for the calculation to fail
                Error err
            | _ ->
                let okList = 
                    aListOfResults 
                    |> List.map (fun item -> 
                                          match item with
                                          | Ok value -> value
                                          | _        -> String.Empty 
                                )   
                Ok okList 

module TryWithRF =

    let internal optionToResultPrint f fPrint : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error fPrint    

    let internal tryWithLazy pfPrint f2 f1 : Result<'a, Lazy<unit>> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error <| lazy (pfPrint (string ex)) 

    let internal optionToResult f err : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error err    
           
    let internal tryWith f2 f1  : Result<'a, string> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error "Chyba v průběhu stahování JŘ DPO nebo JŘ MDPO nebo JŘ KODIS."//(string ex)

module Option = 

    let internal ofNull (value: 'nullableValue) =
        match System.Object.ReferenceEquals(value, null) with //The "value" type can be even non-nullable, and the library method will still work.
        | true  -> None
        | false -> Some value

    let internal ofObj value =
        match value with
        | null -> None
        | _    -> Some value

    let internal ofNullable (value: System.Nullable<'T>) =
        match value.HasValue with
        | true  -> Some value.Value
        | false -> None

    let internal toResult err f : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error err    

    //************************************************************************

    (*
    The inline keyword in F# is primarily used for inlining code at the call site, which can lead to 
    performance improvements in situations where performance optimization is crucial. The impact of inlining 
    is most pronounced when working with generic functions and operations that involve value types.  
    Inline functions are particularly useful when working with collections and higher-order functions.
    Inline functions are a powerful feature that allows the F# compiler to generate specialized code for
    a function at the call site. This can result in more efficient and optimized code, especially when working 
    with generic functions.
    *)

    let inline internal toSrtp (printError: Lazy<unit>) (srtp: ^a) value = 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value
            | None       -> 
                            printError.Force() 
                            srtp  
                            
module Casting = 
    
    //srpt resolved at compile time, generics at run time
    let inline internal castAs<^a> (o: obj) : ^a option =    //the :? operator in F# is used for type testing     
        match Option.ofObj o with
        | Some (:? ^a as result) -> Some result
        | _                      -> None

module TryWith =

    let internal tryWith f1 f2 f3 x y = 
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> 
               f3
               Failure (ex.Message, y)      

    let internal deconstructorError fn1 fn2 =  
        fn1       
        do Console.ReadKey() |> ignore 
        fn2
        do System.Environment.Exit(1) 

    let internal deconstructor (printError: string -> unit) =        
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             //deconstructorError <| printError (string ex) <| ()  
                             deconstructorError <| printError "Chyba v některé fázi procesu programu pro stahování JŘ. Ověř, zdali nepoužíváš adresáře, které se odstraňují či do kterých se zapisuje, například zdali nemáš otevřený některý JŘ v dotčených adresářích. " <| ()  
                             y   

module Parsing =
       
       //Int 
       let internal f x = 
           let isANumber = x                                          
           isANumber  
           
       let rec inline internal parseMeInt printError line =
           function            
           | TryParserInt.Int i -> f i 
           | notANumber         ->  
                                   printError notANumber line 
                                   -1 
       
       //DateTime
       let internal f_date x = 
           let isADate = x       
           isADate               
           
       let rec inline internal parseMeDate (printError: string -> unit) =
           function            
           | TryParserDate.Date d -> f_date d 
           | notADate             -> 
                                     printError notADate
                                     DateTime.MinValue
