﻿module MDPO_Submain

open System
open System.IO
open System.Net

open FSharp.Data

open ProgressBarFSharp
open Messages.Messages
open Helpers.ConsoleFixers
//open Messages.MessagesMocking

open ErrorHandling.TryWith
open ErrorHandling.CustomOption

//************************Constants**********************************************************************

let [<Literal>] pathMdpoWeb = @"https://www.mdpo.cz"
let [<Literal>] pathMdpoWebTimetables = @"https://www.mdpo.cz/jizdni-rady" 

//************************Submain functions************************************************************************

let internal client printToConsole1 printToConsole2 =  

    let myClient x = new System.Net.Http.HttpClient() |> (optionToSrtp <| printToConsole1 <| (new System.Net.Http.HttpClient()))    
    tryWith myClient (fun x -> ()) () String.Empty (new System.Net.Http.HttpClient()) |> deconstructor printToConsole2

let internal filterTimetables pathToDir =   
    
    let urlList = 
        [
            pathMdpoWebTimetables           
        ]
    
    urlList
    |> List.collect (fun url -> 
                              let document = FSharp.Data.HtmlDocument.Load(url)        
                           
                              document.Descendants "a"
                              |> Seq.choose (fun htmlNode ->
                                                           htmlNode.TryGetAttribute("href") //inner text zatim nepotrebuji, cisla linek mam resena jinak 
                                                           |> Option.map (fun a -> string <| htmlNode.InnerText(), string <| a.Value())                                           
                                            )      
                              |> Seq.filter (fun (_ , item2) -> item2.Contains @"/qr/" && item2.Contains ".pdf")
                              |> Seq.map (fun (_ , item2)    ->                                                                 
                                                                let linkToPdf = 
                                                                    sprintf"%s%s" pathMdpoWeb item2  //https://www.mdpo.cz // /qr/201.pdf
                                                                let lineName = item2.Replace(@"/qr/", String.Empty)  
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
                                                     deconstructorError <| message.msgParam1 (string ex) <| client.Dispose() 
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