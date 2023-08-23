namespace PatternBuilders

module PattternBuilders =
   
    let private (>>==) condition nextFunc = //(>>==) double ==
        match condition with
        | false -> '0'
        | true  -> nextFunc() 
    
    [<Struct>]
    type internal MyBuilderCC = MyBuilderCC with            
        member _.Bind(condition, nextFunc) = (>>==) <| condition <| nextFunc 
        member _.Return x = x  
    
    let private (>>=) condition nextFunc =
        match fst condition with
        | false -> snd condition
        | true  -> nextFunc()  
    
    [<Struct>]
    type internal MyBuilder = MyBuilder with    
        member _.Bind(condition, nextFunc) = (>>=) <| condition <| nextFunc
        member _.Return x = x