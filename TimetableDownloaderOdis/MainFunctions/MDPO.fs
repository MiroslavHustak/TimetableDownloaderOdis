module WebScraping1_MDPO

open System
open System.IO
open System.Net

//open MDPO_Submain
open MDPO_Submain_RF

open SettingsKODIS
open Messages.Messages
//open Messages.MessagesMocking

open ErrorHandling
open ErrorHandling.TryWith

//************************Main code*******************************************************************************

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
    | DeleteOneODISDirectory
    | CreateFolders
    | FilterDownloadSave    
    | EndProcess

type internal Environment = 
    {
        filterTimetables: string -> Messages -> (string*string) list
        downloadAndSaveTimetables: Http.HttpClient -> Messages -> string -> (string*string) list -> unit
        client: Http.HttpClient 
    }

//quli client neni default
let internal environment: Environment =
    { 
        filterTimetables = filterTimetables 
        downloadAndSaveTimetables = downloadAndSaveTimetables
        client = client (lazy (Messages.Default.msgParam7 "Error4")) Messages.Default.msgParam1 
    }    

let internal webscraping_MDPO pathToDir =  

     //tryWith block is in the main() function  

    let stateReducer (state: State) (message: Messages) (action: Actions) (environment: Environment) =

        let dirList pathToDir = [ sprintf"%s\%s"pathToDir ODIS.Default.odisDir6 ]

        match action with                                                   
        | StartProcess           -> 
                                    let processStartTime x =    
                                        let processStartTime = sprintf "Začátek procesu: %s" <| DateTime.Now.ToString("HH:mm:ss") 
                                        message.msgParam7 processStartTime 
                                    tryWith processStartTime (fun x -> ()) () String.Empty ()
                                    |> deconstructor message.msgParam1

        | DeleteOneODISDirectory ->                                     
                                    let dirName = ODIS.Default.odisDir6                                    
                                    let myDeleteFunction x =  
                                        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
                                        let dirInfo = new DirectoryInfo(pathToDir) |> Option.toGenerics (lazy (message.msgParam7 "Chyba v průběhu odstraňování starých JŘ MDPO.")) (new DirectoryInfo(pathToDir))   
                                        dirInfo.EnumerateDirectories()
                                        |> Option.toGenerics (lazy (message.msgParam7 "Chyba v průběhu odstraňování starých JŘ MDPO.")) Seq.empty  
                                        |> Seq.filter (fun item -> item.Name = dirName) 
                                        |> Seq.iter (fun item -> item.Delete(true)) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce    
                                    message.msg12()    
                                    tryWith myDeleteFunction (fun x -> ()) () String.Empty () 
                                    |> deconstructor message.msgParam1   
                                    
        | CreateFolders          -> 
                                    let myFolderCreation x = 
                                        dirList pathToDir
                                        |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)                    
                                    tryWith myFolderCreation (fun x -> ()) () String.Empty ()
                                    |> deconstructor message.msgParam1  

        | FilterDownloadSave     -> 
                                    //filtering timetable links, downloading and saving timetables in the pdf format 
                                    let filterDownloadSave x = 
                                        let pathToSubdir = dirList pathToDir |> List.head    
                                        match pathToSubdir |> Directory.Exists with 
                                        | false ->                                              
                                                   message.msgParam5 pathToSubdir   
                                                   message.msg1()                                                
                                        | true  -> 
                                                   environment.filterTimetables pathToSubdir message
                                                   |> environment.downloadAndSaveTimetables environment.client message pathToSubdir  
                                    tryWith filterDownloadSave (fun x -> ()) () String.Empty () 
                                    |> deconstructor message.msgParam1 

                                    environment.client.Dispose()
                                               
        | EndProcess             -> 
                                    let processEndTime x =    
                                        let processEndTime = sprintf "Konec procesu: %s" <| DateTime.Now.ToString("HH:mm:ss")                       
                                        message.msgParam7 processEndTime
                                    tryWith processEndTime (fun x -> ()) () String.Empty () 
                                    |> deconstructor message.msgParam1
    
    stateReducer State.Default Messages.Default StartProcess environment
    stateReducer State.Default Messages.Default DeleteOneODISDirectory environment
    stateReducer State.Default Messages.Default CreateFolders environment
    stateReducer State.Default Messages.Default FilterDownloadSave environment
    stateReducer State.Default Messages.Default EndProcess environment

    environment.client.Dispose()