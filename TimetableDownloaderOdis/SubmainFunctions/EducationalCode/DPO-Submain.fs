module DPO_Submain

open System
open System.IO
open System.Net

open FSharp.Data

open SettingsDPO
open ProgressBarFSharp
open Messages.Messages
open Helpers.ConsoleFixers
//open Messages.MessagesMocking

open ErrorHandling
open ErrorHandling.TryWith


//************************Submain functions************************************************************************

let internal client printToConsole1 printToConsole2 =  

    let myClient x = new System.Net.Http.HttpClient() |> (Option.toGenerics <| printToConsole1 <| (new System.Net.Http.HttpClient()))    
    tryWith myClient (fun x -> ()) (new System.Net.Http.HttpClient()) |> deconstructor printToConsole2

[<TailCall>]
let internal filterTimetables pathToDir (message: Messages) = //I

    let getLastThreeCharacters input =
        match String.length input <= 3 with
        | true  -> 
                   message.msgParam6 input 
                   input 
        | false -> input.Substring(input.Length - 3)

    let removeLastFourCharacters input =
        match String.length input <= 4 with
        | true  -> 
                   message.msgParam6 input 
                   String.Empty
        | false -> input.[..(input.Length - 5)]                    
    
    let urlList = 
        [
            pathDpoWebTimetablesBus      
            pathDpoWebTimetablesTrBus
            pathDpoWebTimetablesTram
        ]
    
    urlList
    |> List.collect (fun url -> 
                              let document = 
                                  let myDocument x = FSharp.Data.HtmlDocument.Load(url)  
                                  tryWith myDocument (fun x -> ()) (FSharp.Data.HtmlDocument.Load(@"https://google.com")) |> deconstructor message.msgParam7
                                                             
                              document.Descendants "a"
                              |> Seq.choose (fun htmlNode ->
                                                           htmlNode.TryGetAttribute("href") //inner text zatim nepotrebuji, cisla linek mam resena jinak  
                                                           |> Option.map (fun a -> string <| htmlNode.InnerText(), string <| a.Value())                                          
                                            )  
                              |> Seq.filter (fun (_ , item2) -> item2.Contains @"/jr/" && item2.Contains ".pdf" && not (item2.Contains "AE-eng.pdf"))
                              |> Seq.map (fun (_ , item2)    ->  
                                                                let linkToPdf = 
                                                                    sprintf"%s%s" pathDpoWeb item2  //https://www.dpo.cz // /jr/2023-04-01/024.pdf
                                                                let adaptedLineName =
                                                                    let s = item2.Replace(@"/jr/", String.Empty).Replace(@"/", "?").Replace(".pdf", String.Empty) 
                                                                    let rec x s =                                                                            
                                                                        match (getLastThreeCharacters s).Contains("?") with
                                                                        | true  -> x <| sprintf "%s%s" s "_"                                                                             
                                                                        | false -> s
                                                                    x s
                                                                let lineName = 
                                                                    let s = sprintf"%s_%s" (getLastThreeCharacters adaptedLineName) adaptedLineName  
                                                                    let s1 = removeLastFourCharacters s 
                                                                    sprintf"%s%s" s1 ".pdf"
                                                                let pathToFile = 
                                                                    sprintf "%s/%s" pathToDir lineName
                                                                linkToPdf, pathToFile
                                         )
                              |> Seq.toList
                              |> List.distinct
                    )    

let internal downloadAndSaveTimetables client (message: Messages) (pathToDir: string) (filterTimetables: (string*string) list) =  

    let downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) =           
               
            async
                {   
                    try //muj custom made tryWith nezachyti exception u async
                        let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                        use fileStream = new FileStream(path, FileMode.CreateNew)                                 
                        return! stream.CopyToAsync(fileStream) |> Async.AwaitTask 
                    with 
                    | :? AggregateException as ex -> 
                                                     message.msgParam2 uri 
                                                     return()                                              
                    | ex                          -> 
                                                     //deconstructorError <| message.msgParam1 (string ex) <| client.Dispose() 
                                                     deconstructorError <| message.msgParam1 "Chyba v průběhu stahování JŘ DPO." <| client.Dispose()
                                                     return()                                
                }  
    
    message.msgParam3 pathToDir 

    let downloadTimetables client = 
        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->  
                                                 let dispatch = 
                                                     async                                                 
                                                         {
                                                             progressBarContinuous message i l  //progressBarContinuous
                                                             async { return! downloadFileTaskAsync client link pathToFile } |> Async.RunSynchronously
                                                         }
                                                 Async.StartImmediate dispatch 
                      )    

    downloadTimetables client 
    
    message.msgParam4 pathToDir
