namespace Helpers

open System
open System.IO
open System.Diagnostics

open Messages.Messages
//open Messages.MessagesMocking

open ErrorHandling
open FreeMonads.FreeMonads
open PatternBuilders.PattternBuilders
   
module ConsoleFixers = 

    let internal consoleAppProblemFixer() =

        do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)    
        
        //Console window settings
        Console.BackgroundColor <- ConsoleColor.Blue 
        Console.ForegroundColor <- ConsoleColor.White 
        Console.InputEncoding   <- System.Text.Encoding.Unicode
        Console.OutputEncoding  <- System.Text.Encoding.Unicode
        Console.WindowWidth     <- 140
        Console.WindowHeight    <- 35

module LogicalAliases =         

    let internal xor a b = (a && not b) || (not a && b)   

    let rec internal nXor operands =
        match operands with
        | []    -> false  
        | x::xs -> (x && not (nXor xs)) || ((not x) && (nXor xs))

module CopyingOrMovingFiles =    //not used yet   
         
    let private processFile source destination message action =

        let sourceFilepath =
            Path.GetFullPath(source)
            |> Option.toGenerics (lazy (message.msgParam7 "Chyba při čtení cesty k souboru")) String.Empty 

        let destinFilepath =
            Path.GetFullPath(destination) 
            |> Option.toGenerics (lazy (message.msgParam7 "Chyba při čtení cesty k")) String.Empty                 
       
        MyBuilder
            {
                let fInfodat: FileInfo = new FileInfo(sourceFilepath)  
                let! _ = fInfodat.Exists, message.msgParam11 source 
                let dInfodat: DirectoryInfo = new DirectoryInfo(destinFilepath) 
                let! _ = dInfodat.Exists, message.msgParam12 source 

                return action sourceFilepath destinFilepath
            }
        
    //to be wrapped in a tryWith block
    //not used yet
    let internal copyFiles source destination message =
        let action sourceFilepath destinFilepath = File.Copy(sourceFilepath, destinFilepath, true)                
        processFile source destination message action
            
    //to be wrapped in a tryWith block
    //not used yet
    let internal moveFiles source destination message =
        let action sourceFilepath destinFilepath = File.Move(sourceFilepath, destinFilepath, true)                
        processFile source destination message action

module CopyingOrMovingFilesFreeMonad =   //not used yet  
        
    [<Struct>]
    type private Config =
        {
            source: string
            destination: string
            fileName: string
        }

    [<Struct>]
    type private IO = 
        | Copy
        | Move 

    let rec private interpret config io = 

        let source = config.source
        let destination = config.destination

        let msg = sprintf "Chyba %s při čtení cesty " 
        
        let result path1 path2 = 
            match path1 with
            | Ok path1  -> 
                        path1
            | Error err -> 
                        printf "%s%s" err path2 
                        Console.ReadKey() |> ignore 
                        System.Environment.Exit(1) 
                        String.Empty

        let f = 
            match io with
            | Copy -> fun p1 p2 -> File.Copy(p1, p2, true) //(fun _ _ -> ())           
            | Move -> fun p1 p2 -> File.Move(p1, p2, true) //(fun _ _ -> ())
      
        function
        | Pure x -> x
        | Free (SourceFilepath next) ->
                                      let sourceFilepath source =                                        
                                          pyramidOfDoom
                                             {
                                                 let! value = Path.GetFullPath(source) |> Option.ofNull, Error <| msg "č.2"   
                                                 let! value = 
                                                     (
                                                         let fInfodat: FileInfo = new FileInfo(value)   
                                                         Option.fromBool value fInfodat.Exists
                                                     ), Error <| msg "č.1"
                                                 return Ok value
                                             }      
                                      next (result (sourceFilepath source) source) |> interpret config io
        | Free (DestinFilepath next) ->
                                      let destinFilepath destination =                                        
                                          pyramidOfDoom
                                             {
                                                 let! value = Path.GetFullPath(destination) |> Option.ofNull, Error <| msg "č.4"   
                                                 let! value = 
                                                     (
                                                         let dInfodat: DirectoryInfo = new DirectoryInfo(value)   
                                                         Option.fromBool value dInfodat.Exists
                                                     ), Error <| msg "č.3"
                                                 return Ok value
                                             }                                        
                                      next (result (destinFilepath destination) destination) |> interpret config io
        | Free (CopyOrMove (s, _))   -> 
                                      let sourceFilepath = fst s
                                      let destinFilepath = snd s  
                                      f sourceFilepath destinFilepath 
                                      //next |> interpret config 
    
    let private config = 
        {
            source = @"e:\UVstarterLog\log.txt" //kontrola s FileInfo
            destination = @"e:\UVstarterLog\test\" //kontrola s DirectoryInfo
            fileName = "test.txt"
        }   

    let private copyOrMoveFiles config io =
        
        cmdBuilder 
            {
                let! sourceFilepath = Free (SourceFilepath Pure)                
                let! destinFilepath = Free (DestinFilepath Pure) 
                return! Free (CopyOrMove ((sourceFilepath, sprintf "%s%s" (destinFilepath) config.fileName), Pure ()))
            } |> interpret config io

    let copyFiles () = copyOrMoveFiles config Copy
    let moveFiles () = copyOrMoveFiles config Move

       
module MyString = 
        
    //priklad pouziti: getString(8, "0")//tuple a compiled nazev velkym kvuli DLL pro C#
    [<CompiledName "GetString">] 
    let getString (numberOfStrings: int, stringToAdd: string): string =   
        let initialString = String.Empty   //initial value of the string
        let listRange = [ 1 .. numberOfStrings ]            
        let rec loop list acc =
            match list with 
            | []        ->
                         acc
            | _ :: tail -> 
                         let finalString = (+) acc stringToAdd  
                         loop tail finalString  //Tail-recursive function calls that have their parameters passed by the pipe operator are not optimized as loops #6984
    
        loop listRange initialString
         

    