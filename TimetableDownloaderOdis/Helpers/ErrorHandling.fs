﻿namespace ErrorHandling

open System
open System.IO

open Parsing
open DiscriminatedUnions

module TryWithRF =

    let inline internal optionToResultPrint f fPrint : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error fPrint    

    let inline internal tryWithLazy pfPrint f2 f1 : Result<'a, Lazy<unit>> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error <| lazy (pfPrint (string ex)) 

    let inline internal optionToResult f err : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error err    
           
    let inline internal tryWith f2 f1  : Result<'a, string> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error (string ex)

module Option = 

    let inline internal ofNull (value: 'nullableValue) =
        match System.Object.ReferenceEquals(value, null) with //The "value" type can be even non-nullable, and the library method will still work.
        | true  -> None
        | false -> Some value

    let inline internal ofObj value =
        match value with
        | null -> None
        | _    -> Some value

    let inline internal ofNullable (value: System.Nullable<'T>) =
        match value.HasValue with
        | true  -> Some value.Value
        | false -> None

    //************************************************************************

    let inline internal toSrtp (printError: Lazy<unit>) (srtp: ^a) value = 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value
            | None       -> 
                            printError.Force() 
                            srtp  
                            
module Casting = 
    
    let castAs<'a> (o: obj) : 'a option =    //the :? operator in F# is used for type testing     
        match Option.ofObj o with
        | Some (:? 'a as result) -> Some result
        | _                      -> None

module TryWith =

    let inline internal tryWith f1 f2 f3 x y = 
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
                             deconstructorError <| printError (string ex) <| ()                             
                             y   

module Parsing =
       
       //Int 
       let inline internal f x = 
           let isANumber = x                                          
           isANumber  
           
       let rec inline internal parseMeInt printError line =
           function            
           | TryParserInt.Int i -> f i 
           | notANumber         ->  
                                   printError notANumber line 
                                   -1 
       
       //DateTime
       let inline internal f_date x = 
           let isADate = x       
           isADate               
           
       let rec inline internal parseMeDate (printError: string -> unit) =
           function            
           | TryParserDate.Date d -> f_date d 
           | notADate             -> 
                                     printError notADate
                                     DateTime.MinValue
