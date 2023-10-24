namespace FreeMonads

module FreeMonads =
           
    type internal CommandLineInstruction<'a> =
        | SourceFilepath of (string -> 'a)
        | DestinFilepath of (string -> 'a)
        | CopyOrMove of (string * string) * 'a

    type internal CommandLineProgram<'a> =
        | Pure of 'a 
        | Free of CommandLineInstruction<CommandLineProgram<'a>>

    let internal mapI f = 
        function
        | SourceFilepath next  -> SourceFilepath (next >> f)
        | DestinFilepath next  -> DestinFilepath (next >> f)
        | CopyOrMove (s, next) -> CopyOrMove (s, next |> f)    

    let rec internal bind f = 
        function
        | Free x -> x |> mapI (bind f) |> Free
        | Pure x -> f x

    
   