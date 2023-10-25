namespace CEBuilders

open FreeMonads.FreeMonadsCM

module PattternBuilders =
           
    [<Struct>]
    type internal MyBuilderCC = MyBuilderCC with    //zatim nevyuzito        
        member _.Bind(resultFn, nextFunc) = 
            match resultFn with
            | Ok resultFn -> Ok <| nextFunc() 
            | Error err   -> Error err
        member _.Return x = x  


    //**************************************************************************************

    [<Struct>]
    type internal MyBuilder = MyBuilder with    
        member _.Bind(condition, nextFunc) =
            match fst condition with
            | false -> snd condition
            | true  -> nextFunc()  
        member _.Return x = x

    let internal pyramidOfHell = MyBuilder


    //**************************************************************************************

    [<Struct>]
     type internal Builder2 = Builder2 with    
         member _.Bind((optionExpr, err), nextFunc) =
             match optionExpr with
             | Some value -> nextFunc value 
             | _          -> err  
         member _.Return x : 'a = x

     let internal pyramidOfDoom = Builder2


    //************************** FOR A READER MONAD ************************************************************

    // Define a type alias for the reader monad    
    type internal Reader<'e, 'a> = 'e -> 'a

    [<Struct>] 
    type internal ReaderBuilder = ReaderBuilder with
        member __.Bind(m, f) = fun env -> f (m env) env      
        member __.Return x = fun _ -> x
        member __.ReturnFrom x = x
        member __.Zero x = x
    
    let internal reader = ReaderBuilder 


    //************************** FOR A FREE MONAD **************************************************************

                           //See FreeMonads modules