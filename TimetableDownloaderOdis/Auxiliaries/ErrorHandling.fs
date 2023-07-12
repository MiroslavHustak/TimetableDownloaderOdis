namespace ErrorHandling

open System
open System.IO

open Parsing
open DiscriminatedUnions

module TryWithRF =

    let inline optionToResultPrint f fPrint : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error fPrint    

    let inline tryWithLazy pfPrint f2 f1 : Result<'a, Lazy<unit>> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error <| lazy (pfPrint (string ex)) 

    let inline optionToResult f err : Result<'a, 'b> = 
        f                      
        |> function   
            | Some value -> Ok value 
            | None       -> Error err    
           
    let inline tryWith f2 f1  : Result<'a, string> =            
        try
            try                 
                f2
            finally
                f1
        with
        | ex -> Error (string ex)

module CustomOption = 
        
    let inline optionToSrtp (printError: Lazy<unit>) (srtp: ^a) value = 
        value
        |> Option.ofObj 
        |> function 
            | Some value -> value
            | None       -> 
                            printError.Force() 
                            srtp    

module TryWith =

    let inline tryWith f1 f2 f3 x y = 
        try
            try          
               f1 x |> Success
            finally
               f2 x
        with
        | ex -> 
               f3
               Failure (ex.Message, y)      

    let deconstructorError fn1 fn2 =  
        fn1       
        do Console.ReadKey() |> ignore 
        fn2
        do System.Environment.Exit(1) 

    let deconstructor (printError: string -> unit) =        
        function
        | Success x       -> x                                                   
        | Failure (ex, y) -> 
                             deconstructorError <| printError (string ex) <| ()                             
                             y   

module Parsing =
       
       //Int 
       let inline f x = 
           let isANumber = x                                          
           isANumber  
           
       let rec inline parseMeInt (printError: string -> unit) =
           function            
           | TryParserInt.Int i -> f i 
           | notANumber         ->  
                                   printError notANumber 
                                   -1 
       
       //DateTime
       let inline f_date x = 
           let isADate = x       
           isADate               
           
       let rec inline parseMeDate (printError: string -> unit) =
           function            
           | TryParserDate.Date d -> f_date d 
           | notADate             -> 
                                     printError notADate
                                     DateTime.MinValue
