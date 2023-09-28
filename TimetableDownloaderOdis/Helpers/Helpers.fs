namespace Helpers

open System
open System.IO
open System.Diagnostics

open Messages.Messages
//open Messages.MessagesMocking

open ErrorHandling
   
module ConsoleFixers = 

    let internal consoleAppProblemFixer() =
        do System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)        
        Console.BackgroundColor <- ConsoleColor.Blue 
        Console.ForegroundColor <- ConsoleColor.White 
        Console.InputEncoding   <- System.Text.Encoding.Unicode
        Console.OutputEncoding  <- System.Text.Encoding.Unicode

module LogicalAliases =         

    let internal xor a b = (a && not b) || (not a && b) //P    

module CopyingOrMovingFiles =    //not used yet   
         
    let private processFile source destination message action =
        let sourceFilepath =
            Path.GetFullPath(source)
            |> Option.toGenerics (lazy (message.msgParam7 "Chyba při čtení cesty k souboru")) String.Empty 
        let destinFilepath =
            Path.GetFullPath(destination) 
            |> Option.toGenerics (lazy (message.msgParam7 "Chyba při čtení cesty k souboru")) String.Empty                 
        let fInfodat: FileInfo = new FileInfo(sourceFilepath)  
        match fInfodat.Exists with 
        | true  -> action sourceFilepath destinFilepath
        | false -> message.msgParam11 source  
        
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
       
module MyString = 
        
    [<CompiledName "GetString">]  //priklad pouziti: getString(8, "0")//tuple a compiled nazev velkym kvuli DLL pro C#
    let getString (numberOfStrings: int, stringToAdd: string): string =   //P
        let initialString = String.Empty                //initial value of the string
        let listRange = [ 1 .. numberOfStrings ]
        let rec loop list acc auxStringToAdd =
            match list with 
            | []        -> acc
            | _ :: tail -> 
                            let finalString = (+) acc auxStringToAdd
                            loop tail finalString auxStringToAdd //Tail-recursive function calls that have their parameters passed by the pipe operator are not optimized as loops #6984
        loop listRange initialString stringToAdd //Tail-recursive function calls ... viz vyse   
         

    