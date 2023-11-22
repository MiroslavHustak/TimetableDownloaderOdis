module WebScraping1_KODISFM

open System
open System.IO
open System.Net

open KODIS_Submain
open Messages.Messages
//open Messages.MessagesMocking

open DiscriminatedUnions
open FreeMonads.FreeMonadsDP 

open ErrorHandling
open ErrorHandling.TryWith
open CEBuilders.PattternBuilders

type internal State =  //not used
    { 
        TimetablesDownloadedAndSaved: unit
    }
    static member Default = 
        {          
            TimetablesDownloadedAndSaved = ()
        }
           
type internal Actions =
    | StartProcess
    | DownloadAndSaveJson
    | DownloadSelectedVariant of Validity list           
    | EndProcess

type internal Environment = 
    {
        deleteOneODISDirectory: Messages -> Validity -> string -> unit 
        deleteAllODISDirectories: Messages -> string -> unit 
        createFolders: Messages -> string list -> unit 
        downloadAndSave: Messages -> Validity -> string -> Http.HttpClient -> unit        
        client: Http.HttpClient 
    }

//quli client neni default
let internal environment: Environment =
    { 
        deleteOneODISDirectory = deleteOneODISDirectory
        deleteAllODISDirectories = deleteAllODISDirectories
        createFolders = createFolders
        downloadAndSave = downloadAndSave       
        client = client (lazy (Messages.Default.msgParam7 "Chyba v průběhu stahování JŘ KODIS.")) Messages.Default.msgParam1 
    }  

//[<TailCall>]
let internal webscraping_KODISFM pathToDir (variantList: Validity list) = 
    
    //****************************MainFunction**********************************   
    
    let stateReducer (state: State) (message: Messages) (action: Actions) (environment: Environment) =

        match action with                                                   
        | StartProcess                        -> 
                                                 let processStartTime x =    
                                                     let processStartTime = sprintf "Začátek procesu: %s" <| DateTime.Now.ToString("HH:mm:ss") 
                                                     message.msgParam7 processStartTime 
                                                 tryWith processStartTime (fun x -> ()) ()
                                                 |> deconstructor message.msgParam1

        | DownloadAndSaveJson                 -> downloadAndSaveJson message environment.client  //try with included
            
        | DownloadSelectedVariant variantList -> 
                                                 let downloadSelectedVariant x = 
                                                     match variantList |> List.length with
                                                     //SingleVariantDownload
                                                     | 1 -> 
                                                          let variant = variantList |> List.head
                                                          environment.deleteOneODISDirectory message variant pathToDir                                                        
                                                          let dirList = 
                                                              createOneNewDirectory  //list -> aby bylo mozno pouzit funkci createFolders bez uprav  
                                                              <| pathToDir 
                                                              <| createDirName variant getDefaultRcValKodis 
                                                          environment.createFolders message dirList
                                                          environment.downloadAndSave message variant (dirList |> List.head) environment.client 

                                                     //BulkVariantDownload       
                                                     | _ ->  
                                                          environment.deleteAllODISDirectories message pathToDir
                                                          let dirList = createNewDirectories pathToDir getDefaultRcValKodis
                                                          environment.createFolders message dirList 
                                                          (variantList, dirList)
                                                          ||> List.iter2 (fun variant dir -> environment.downloadAndSave message variant dir environment.client)         
                                                 
                                                 tryWith downloadSelectedVariant (fun x -> ()) ()
                                                 |> deconstructor message.msgParam1  
                                                 
                                                 environment.client.Dispose()
           
        | EndProcess                         -> 
                                                let processEndTime x =    
                                                    let processEndTime = sprintf "Konec procesu: %s" <| DateTime.Now.ToString("HH:mm:ss")                       
                                                    message.msgParam7 processEndTime
                                                tryWith processEndTime (fun x -> ()) () 
                                                |> deconstructor message.msgParam1       
    
    let rec interpret = //Free Monad for educational purposes
        function
        | Pure x                                -> x
        | Free (StartProcessFM next)            -> stateReducer State.Default Messages.Default StartProcess environment
                                                   next () |> interpret
        | Free (DownloadAndSaveJsonFM next)     -> stateReducer State.Default Messages.Default DownloadAndSaveJson environment
                                                   next () |> interpret
        | Free (DownloadSelectedVariantFM next) -> stateReducer State.Default Messages.Default (DownloadSelectedVariant variantList) environment
                                                   next () |> interpret
        | Free (EndProcessFM _)                 -> stateReducer State.Default Messages.Default EndProcess environment   
     
    cmdBuilder
        {
            let! _ = Free (StartProcessFM Pure)
            let! _ = Free (DownloadAndSaveJsonFM Pure)
            let! _ = Free (DownloadSelectedVariantFM Pure)
            return! Free (EndProcessFM Pure)
        } |> interpret

    //*****************************************************************************************************************************************

    //CurrentValidity = JR striktne platne k danemu dni, tj. pokud je napr. na dany den vylukovy JR, stahne se tento JR, ne JR platny dalsi den
    //FutureValidity = JR platne v budouci dobe, ktere se uz vyskytuji na webu KODISu
    //ReplacementService = pouze vylukove JR, JR NAD a JR X linek
    //WithoutReplacementService = JR dlouhodobe platne bez jakykoliv vyluk. Tento vyber neobsahuje ani dlouhodobe nekolikamesicni vyluky, muze se ale hodit v pripade, ze zakladni slozka s JR obsahuje jedno ci dvoudenni vylukove JR.     