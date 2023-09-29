module MDPO_Submain_RF

open System
open System.IO
open System.Net
open System.Net.Http
open System.Reflection
open System.Net.NetworkInformation

open FSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.FSharp.Reflection

open SettingsMDPO
open ProgressBarFSharp
open Messages.Messages
//open Messages.MessagesMocking

open ErrorTypes.ErrorTypes

open ErrorHandling
open ErrorHandling.TryWith
open ErrorHandling.TryWithRF


//************************Submain helpers**************************************************************************

let private getDefaultRcVal (t: Type) (r: ConnErrorCode) =  //reflection nefunguje s type internal
    
    FSharpType.GetRecordFields(t) 
    |> Array.map (fun (prop: PropertyInfo) -> 
                                            match Casting.castAs<string> <| prop.GetValue(r) with
                                            | Some value -> Ok value
                                            | None       -> Error "Chyba v průběhu stahování JŘ MDPO." 
                 ) |> List.ofArray |> Result.sequence          
            
let private getDefaultRecordValues = 

    try   
        getDefaultRcVal typeof<ConnErrorCode> ConnErrorCode.Default 
    with
    | ex -> Error "Chyba v průběhu stahování JŘ MDPO." 

//************************Submain functions************************************************************************

let internal client (printToConsole1 : Lazy<unit>) (printToConsole2: string -> unit) : HttpClient = 
    
    let f = new HttpClient() |> Option.ofObj 
    
    tryWithLazy printToConsole2 (optionToResultPrint f printToConsole1) ()           
    |> function    
        | Ok value  -> value
        | Error err -> 
                       err.Force()
                       new System.Net.Http.HttpClient()  

let internal filterTimetables pathToDir (message: Messages) = 
    
    let urlList = 
        [
            pathMdpoWebTimetables
        ]
    
    urlList
    |> List.collect (fun url -> 
                              let document = 
                                  let f = Ok <| FSharp.Data.HtmlDocument.Load(url)   

                                  tryWith f ()          
                                  |> function    
                                      | Ok value -> value
                                      | Error ex -> 
                                                    message.msgParam7 (string ex)     
                                                    Console.ReadKey() |> ignore 
                                                    System.Environment.Exit(1)  
                                                    FSharp.Data.HtmlDocument.Load(@"https://google.com")
                                                    
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

    let downloadFileTaskAsync (client: Http.HttpClient) (uri: string) (path: string) : Async<Result<unit, string>> =  
        
        async
            {                      
                try                       
                    match File.Exists(path) with
                    | true  -> return Ok () 
                    | false -> 
                               let! response = client.GetAsync(uri) |> Async.AwaitTask
                        
                               match response.IsSuccessStatusCode with //true if StatusCode was in the range 200-299; otherwise, false.
                               | true  -> 
                                           let! stream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask    
                                           use fileStream = new FileStream(path, FileMode.CreateNew) 
                                           do! stream.CopyToAsync(fileStream) |> Async.AwaitTask
                                           return Ok ()
                               | false -> 
                                           let errorType = 
                                               match response.StatusCode with
                                               | HttpStatusCode.BadRequest          -> Error "400 Bad Request"
                                               | HttpStatusCode.InternalServerError -> Error "500 Internal Server Error"
                                               | HttpStatusCode.NotImplemented      -> Error "501 Not Implemented"
                                               | HttpStatusCode.ServiceUnavailable  -> Error "503 Service Unavailable"
                                               | HttpStatusCode.NotFound            -> Error uri  
                                               | _                                  -> Error "418 I'm a teapot. Look for a coffee maker elsewhere."                                                                               
                                           return errorType     
                with                                                         
                | ex -> 
                        closeIt client message "Chyba v průběhu stahování JŘ MDPO."//(string ex)                                                 
                        return Error String.Empty    
            }   
    
    message.msgParam3 pathToDir 
    
    let downloadTimetables (client: HttpClient) = 

        let l = filterTimetables |> List.length

        filterTimetables 
        |> List.iteri (fun i (link, pathToFile) ->             
                                                async                                                
                                                    {
                                                        progressBarContinuous message i l  //progressBarContinuous  
                                                        return! downloadFileTaskAsync client link pathToFile                                                                                                                               
                                                    } 
                                                    |> Async.Catch
                                                    |> Async.RunSynchronously
                                                    |> Result.ofChoice                                                    
                                                    |> function                                                 
                                                        | Ok value ->  
                                                                     value 
                                                                     |> function
                                                                         | Ok value  -> ()
                                                                         | Error err -> 
                                                                                     getDefaultRecordValues
                                                                                     |> function
                                                                                         | Ok value ->
                                                                                                     value
                                                                                                     |> List.tryFind (fun item -> err = item)
                                                                                                     |> function
                                                                                                         | Some err -> closeIt client message err                                                                      
                                                                                                         | None     -> message.msgParam2 link 
                                                                                         | Error err ->
                                                                                                      closeIt client message err                                                                                  
                                                        | Error _  -> message.msgParam2 link                     
                      )    

    downloadTimetables client 
    
    message.msgParam4 pathToDir




