namespace FreeMonads

module FreeMonadsCM =

//***************************Copy/Move********************************
           
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
     
    [<TailCall>]
    let rec internal bind f = 
        function
        | Free x -> x |> mapI (bind f) |> Free
        | Pure x -> f x

    type internal CommandLineProgramBuilder = CommandLineProgramBuilder with
        member this.Bind(p, f) = //x |> mapI (bind f) |> Free
            match p with
            | Pure x     -> f x
            | Free instr -> Free (mapI (fun p' -> this.Bind(p', f)) instr)
        member this.Return x = Pure x
        member this.ReturnFrom p = p

    let internal cmdBuilder = CommandLineProgramBuilder 
   

 module FreeMonadsDP =   

//***************************KODIS Design Pattern********************************  

    type internal CommandLineInstruction<'a> =
        | StartProcessFM of (unit -> 'a)
        | DownloadAndSaveJsonFM of (unit -> 'a)
        | DownloadSelectedVariantFM of (unit -> 'a)
        | EndProcessFM of (unit -> 'a)

    type internal CommandLineProgram<'a> =
        | Pure of 'a
        | Free of CommandLineInstruction<CommandLineProgram<'a>>

    let internal mapI f =
        function
        | StartProcessFM next            -> StartProcessFM (next >> f)
        | DownloadAndSaveJsonFM next     -> DownloadAndSaveJsonFM (next >> f)
        | DownloadSelectedVariantFM next -> DownloadSelectedVariantFM (next >> f)
        | EndProcessFM next              -> EndProcessFM (next >> f)
    
    [<TailCall>]
    let rec internal bind f program =
        match program with
        | Free x -> Free (mapI (bind f) x)
        | Pure x -> f x

    type internal CommandLineProgramBuilder = CommandLineProgramBuilder with
        member this.Bind(p, f) = //x |> mapI (bind f) |> Free
            match p with
            | Pure x     -> f x
            | Free instr -> Free (mapI (fun p' -> this.Bind(p', f)) instr)
        member this.Return x = Pure x
        member this.ReturnFrom p = p

    let internal cmdBuilder = CommandLineProgramBuilder  

   
   