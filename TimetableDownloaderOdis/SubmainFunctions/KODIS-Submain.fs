module KODIS_Submain

open System
open System.IO
open System.Net
open System.Reflection
open System.Text.RegularExpressions

open Fugit
open FSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.FSharp.Reflection

open SettingsKODIS
open Messages.Messages
open ProgressBarFSharp
open DiscriminatedUnions
open Helpers.LogicalAliases
open PatternBuilders.PattternBuilders

open ErrorHandling
open ErrorHandling.TryWith
open ErrorHandling.Parsing

type internal KodisTimetables = JsonProvider<pathJson> 

//*********************Helpers*******************************************

let private getDefaultRcVal (t: Type) (r: ODIS) itemNo = 

   //reflection nefunguje s type internal
   
   try 
       FSharpType.GetRecordFields(t) 
       |> Array.map
           (fun (prop: PropertyInfo) -> 
                                      match Casting.castAs<string> <| prop.GetValue(r) with
                                      | Some value -> value
                                      | None       -> failwith "Chyba v průběhu stahování JŘ KODIS." //vyjimecne ponechavam takto, bo se mi to nechce predelavat na message.msgParamX
                                            (*
                                                For educational purposes
                                                match prop.GetValue(r) with
                                                | :? string as str -> str //the :? operator in F# is used for type testing and downcasting
                                                | _                -> failwith "Error" 
                                            *)
           )            
       |> List.ofArray 
       |> List.take itemNo     
   
   with
   | ex -> failwith "Chyba v průběhu stahování JŘ KODIS." //vyjimecne ponechavam takto, bo se mi to nechce predelavat na message.msgParamX, chyba je stejne malo pravdepodobna    

let private splitList message list = 

    let mySplitting x = 
        let folder (a: string, b: string) (cur, acc) =
            let cond = a.Substring(0, lineNumberLength) = b.Substring(0, lineNumberLength) 
            match a with
            | _ when cond -> a::cur, acc
            | _           -> [a], cur::acc
        let result = List.foldBack folder (List.pairwise list) ([ List.last list ], []) 
        (fst result)::(snd result)
    tryWith mySplitting (fun x -> ()) () String.Empty [ [] ] |> deconstructor message.msgParam1

    (*
    splitList will split the input list into groups of adjacent elements that have the same prefix.
    splitListByPrefix will group together all elements that have the same prefix, regardless of whether they are adjacent in the input list or not.
    *)

let private splitListByPrefix message (list: string list) : string list list = 

    let mySplitting x = 
        let prefix = (fun (x: string) -> x.Substring(0, lineNumberLength))
        let groups = list |> List.groupBy prefix  
        let filteredGroups = groups |> List.filter (fun (k, _) -> k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength))
        let result = filteredGroups |> List.map snd
        result
    tryWith mySplitting (fun x -> ()) () String.Empty [ [] ] |> deconstructor message.msgParam1

//ekvivalent splitListByPrefix za predpokladu existence teto podminky shodnosti k.Substring(0, lineNumberLength) = k.Substring(0, lineNumberLength)   
let private splitList1 (list: string list) : string list list = 

    list |> List.groupBy (fun (item: string) -> item.Substring(0, lineNumberLength)) |> List.map (fun (key, group) -> group) 

let private getDefaultRecordValues = 

    try   
        getDefaultRcVal typeof<ODIS> ODIS.Default 4 //jen prvni 4 polozky jsou pro celo-KODIS variantu
    with
    | ex -> failwith "Chyba v průběhu stahování JŘ KODIS." //vyjimecne ponechavam takto, bo se mi to nechce predelavat na message.msgParamX, chyba je stejne malo pravdepodobna 
 

//************************Main code***********************************************************

let internal client printToConsole1 printToConsole2 =  

    let myClient x = new System.Net.Http.HttpClient() |> (Option.toGenerics <| printToConsole1 <| (new System.Net.Http.HttpClient()))    
    tryWith myClient (fun x -> ()) () String.Empty (new System.Net.Http.HttpClient()) |> deconstructor printToConsole2

let internal downloadAndSaveJson2 message (client: Http.HttpClient) = //ponechano z vyukovych duvodu 

    //nepouzito, zjistovani delky json souboru trva tady stejne dluho, jako jejich stazeni  
    //v pripade stahovani velkych souboru by uz mohl byt zjevny rozdil, tra vyzkusat    
    
    let updateJson x =          
        
        let loadAndSaveJsonFiles = 

            let l = jsonLinkList |> List.length             

            let fileLengthList = 
                pathToJsonList
                |> List.toArray
                |> Array.Parallel.map 
                    (fun item ->                                          
                                let fileInfo = new FileInfo(Path.GetFullPath(item))
                                                   
                                try   
                                    match fileInfo.Exists with
                                    | true  -> Some fileInfo.Length 
                                    | false -> None                                               
                                with
                                | ex -> 
                                      deconstructorError <| message.msgParam7 "Chyba v průběhu stahování JSON souborů pro JŘ KODIS." <| ()    
                                      None       
                    ) |> List.ofArray

            (jsonLinkList, fileLengthList)
            ||> List.mapi2
                (fun i link length ->                                                
                                    progressBarContinuous message i l 
                                    //updateJson x nezachyti exception v async
                                              
                                    let webJsonLength (url: string) =                                          
                                        async  
                                            { 
                                                try 
                                                    let! httpResponse = client.GetAsync(url) |> Async.AwaitTask
                                                    let response = httpResponse.Content.Headers
                                                              
                                                    let contentLength = 
                                                        response.ContentLength
                                                        |> Option.ofNullable    
                                                        |> Option.map (fun value -> value)
                                                                 
                                                    return contentLength
                                                with
                                                | ex -> 
                                                        //deconstructorError <| message.msgParam7 (string ex) <| () 
                                                        deconstructorError <| message.msgParam7 "Chyba v průběhu stahování JSON souborů pro JŘ KODIS." <| () 
                                                        return None
                                            } |> Async.RunSynchronously                                                               
                                                     
                                    let download() = 
                                        async  
                                            { 
                                                try 
                                                    return! client.GetStringAsync(link) |> Async.AwaitTask 
                                                with
                                                | ex -> 
                                                        //deconstructorError <| message.msgParam1 (string ex) <| ()
                                                        deconstructorError <| message.msgParam1 "Chyba v průběhu stahování JSON souborů pro JŘ KODIS." <| ()
                                                        return! client.GetStringAsync(String.Empty) |> Async.AwaitTask //whatever of that type
                                            } |> Async.RunSynchronously
                                                      
                                    match length, webJsonLength link with
                                    | Some hdLength, Some webLength
                                        when hdLength = webLength + 2L -> nonJsonString                                                                                                                                                                                                       
                                    | _                                -> download()  
                )     
        
        //save updated json files
        match (<>) (pathToJsonList |> List.length) (loadAndSaveJsonFiles |> List.length) with
        | true  ->                   
                  message.msg1()                  
                  do Console.ReadKey() |> ignore 
                  do System.Environment.Exit(1)
        | false ->
                  (pathToJsonList, loadAndSaveJsonFiles)
                  ||> List.iteri2 
                      (fun i path json -> 
                                        match json.Equals(nonJsonString) with
                                        | true  -> ()
                                        | false ->                                                       
                                                    use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                                    streamWriter.WriteLine(json)     
                                                    streamWriter.Flush()   
                      ) 

    message.msg2() 

    tryWith updateJson (fun x -> ()) () String.Empty () |> deconstructor message.msgParam1    

    message.msg3() 
    message.msg4() 

let internal downloadAndSaveJson message (client: Http.HttpClient) = 

    let updateJson x = 
        let loadAndSaveJsonFiles = 
            let l = jsonLinkList |> List.length
            jsonLinkList
            |> List.mapi
                (fun i item ->                                                
                            progressBarContinuous message i l 
                            //updateJson x nezachyti exception v async
                            async  
                                { 
                                    try 
                                        return! client.GetStringAsync(item) |> Async.AwaitTask 
                                    with
                                    | ex -> 
                                          deconstructorError <| message.msgParam1 "Chyba v průběhu stahování JSON souborů pro JŘ KODIS." <| ()
                                          return! client.GetStringAsync(String.Empty) |> Async.AwaitTask //whatever of that type
                                } |> Async.RunSynchronously                        
                )  

        //save updated json files
        match (<>) (pathToJsonList |> List.length) (loadAndSaveJsonFiles |> List.length) with
        | true  -> 
                  message.msg1()
                  do Console.ReadKey() |> ignore 
                  do System.Environment.Exit(1)
        | false ->
                  (pathToJsonList, loadAndSaveJsonFiles)
                  ||> List.iteri2 
                      (fun i path json ->                                                                          
                                        use streamWriter = new StreamWriter(Path.GetFullPath(path))                   
                                        streamWriter.WriteLine(json)     
                                        streamWriter.Flush()   
                      ) 

    message.msg2() 

    tryWith updateJson (fun x -> ()) () String.Empty () |> deconstructor message.msgParam1    

    message.msg3() 
    message.msg4() 
   
let internal digThroughJsonStructure message = //prohrabeme se strukturou json souboru //printfn -> additional 4 parameters
    
    let kodisTimetables () = 

        let myFunction x = 
            pathToJsonList 
            |> Array.ofList 
            |> Array.collect 
                (fun pathToJson ->   
                                let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj //I
                                //let kodisJsonSamples = kodisJsonSamples.GetSample() |> Option.ofObj  //v pripade jen jednoho json               
                
                                kodisJsonSamples 
                                |> function 
                                    | Some value -> value |> Array.map (fun item -> item.Timetable) //quli tomuto je nutno Array
                                    | None       -> 
                                                    message.msg5() 
                                                    [||]    
                ) 
        
        tryWith myFunction (fun x -> ()) () String.Empty [||] |> deconstructor message.msgParam1

    let kodisAttachments () = 

        (*
        //ponechavam pro pochopeni struktury u json type provider (pri pouziti option se to tahne az k susedovi)
        let kodisAttachments() = 
            kodisJsonSamples                              
            |> Array.collect (fun item ->                                            
                                        item.Vyluky 
                                        |> Array.collect (fun item ->                                                 
                                                                    item.Attachments
                                                                    |> Array.Parallel.map (fun item -> item.Url)
                                                         ) 
                             )   
        *)            

        let myFunction x = 
            
            let errorStr str err = str |> (Option.toGenerics <| lazy (message.msgParam7 err) <| String.Empty) 

            pathToJsonList
            |> Array.ofList 
            |> Array.collect
                (fun pathToJson ->
                                let fn1 (value: JsonProvider<pathJson>.Attachment array) = 
                                    value //Option je v errorStr 
                                    |> Array.Parallel.map (fun item -> errorStr item.Url "Chyba v průběhu stahování JSON souborů pro JŘ KODIS.")

                                let fn2 (item: JsonProvider<pathJson>.Vyluky) =  //quli tomuto je nutno Array     
                                    item.Attachments |> Option.ofNull        
                                    |> function 
                                        | Some value -> value |> fn1
                                        | None       -> 
                                                        message.msg6() 
                                                        [||]                 

                                let fn3 (item: JsonProvider<pathJson>.Root) =  //quli tomuto je nutno Array 
                                    item.Vyluky |> Option.ofObj
                                    |> function 
                                        | Some value -> value |> Array.collect fn2 
                                        | None       ->
                                                        message.msg7() 
                                                        [||] 
                                              
                                let kodisJsonSamples = KodisTimetables.Parse(File.ReadAllText pathToJson) |> Option.ofObj 
                                              
                                kodisJsonSamples 
                                |> function 
                                    | Some value -> value |> Array.collect fn3 
                                    | None       -> 
                                                    message.msg8() 
                                                    [||]                                 
                ) 
        
        tryWith myFunction (fun x -> ()) () String.Empty [||] |> deconstructor message.msgParam1   
        
    let addOn () = 
        [
            //pro pripad, kdy KODIS strci odkazy bud do uplne jinak strukturovaneho jsonu, tudiz nelze pouzit dany type provider, anebo je vubec do jsonu neda
            @"https://kodis-files.s3.eu-central-1.amazonaws.com/76_2023_10_09_2023_10_20_v_f2b77c8fad.pdf"
            @"https://kodis-files.s3.eu-central-1.amazonaws.com/64_2023_10_09_2023_10_20_v_02e6717b5c.pdf"            
        ] |> List.toArray 
   
    (Array.append (Array.append <| kodisAttachments () <| kodisTimetables ()) <| addOn()) |> Set.ofArray  
    //(Array.append <| kodisAttachments () <| kodisTimetables ()) |> Set.ofArray //jen z vyukovych duvodu -> konverzi na Set vyhodime stejne polozky, jinak staci jen |> Array.distinct 

    //kodisAttachments() |> Set.ofArray //over cas od casu
    //kodisTimetables() |> Set.ofArray //over cas od casu

let internal filterTimetables message param pathToDir diggingResult  = 

    //****************prvni filtrace odkazu na neplatne jizdni rady***********************        
    
    let myList = 
        let myFunction x =            
            diggingResult
            |> Set.toArray 
            |> Array.Parallel.map
                (fun (item: string) ->   
                                    let item = string item

                                    //******************************************************************************
                                    //misto pro rucni opravu retezcu v PDF, ktere jsou v jsonu v nespravnem formatu ci s chybnym datem 
                                    let item = 
                                        match item.Contains(@"S2_2023_04_03_2023_04_3_v") with
                                        | true  -> item.Replace(@"S2_2023_04_03_2023_04_3_v", @"S2_2023_04_03_2023_04_03_v")  
                                        | false -> item   
                                                        
                                    let item = 
                                        match item.Contains(@"https://kodis-files.s3.eu-central-1.amazonaws.com/55_2023_07_01_2023_09_02_eb08ce03a7.pdf") with
                                        | true  -> item.Replace(@"https://kodis-files.s3.eu-central-1.amazonaws.com/55_2023_07_01_2023_09_02_eb08ce03a7.pdf", @"https://kodis-files.s3.eu-central-1.amazonaws.com/55_2023_07_26_2023_09_03_v_6186b834e8.pdf")  
                                        | false -> item   
                                                            
                                    let item = //X3 s chybnym koncem platnosti lze vyradit jen rucne (zmenou data na skutecny konec platnosti), bo bez podivani se do obsahu nelze urcit, zdali jsou jeste relevantni ci ne
                                        match item.Contains(@"X3_2023_03_07_2023_12_09") || item.Contains(@"X3_2022_12_11_2023_12_09") with
                                        | true  -> item.Replace(@"2023_12_09", @"2023_09_04")  
                                        | false -> item    
                                                            
                                    let item = //X55 s chybnym koncem platnosti lze vyradit jen rucne (zmenou data na skutecny konec platnosti), bo bez podivani se do obsahu nelze urcit, zdali jsou jeste relevantni ci ne
                                        match item.Contains(@"X55_2023_03_07_2023_12_09") || item.Contains(@"X55_2022_12_11_2023_12_09") with
                                        | true  -> item.Replace(@"2023_12_09", @"2023_09_04")  
                                        | false -> item   
                                                            
                                    let item = //X28 s chybnym koncem platnosti lze vyradit jen rucne (zmenou data na skutecny konec platnosti), bo bez podivani se do obsahu nelze urcit, zdali jsou jeste relevantni ci ne
                                        match item.Contains(@"X28_2023_03_07_2023_12_09") with
                                        | true  -> item.Replace(@"2023_12_09", @"2023_09_04")  
                                        | false -> item      
                                    //konec rucni opravy retezcu  
                                    //_X3_2022_12_11_2023_12_09.pdf
                                    //******************************************************************************
                                                                                                                                                         
                                    //az bude cas, implementuj (misto meho reseni nize) tento kod do logiky odstraneni prebytecneho retezce plus jeste odstraneni po normalnim datu 
                                    let replacePattern (input: string) =
                                        let pattern = @"(_v).*?(\.pdf)" 
                                        let replacement = "$1$2"
                                        Regex.Replace(input, pattern, replacement)      
                                                            
                                        //let pattern = @"_v[^.]+\.pdf"
                                        //Regex.Replace(input, pattern, "_v.pdf")

                                    let item = 
                                        match (item.Contains(@"_v") || item.Contains(@"_t")) && item.Contains(@"_.pdf") with
                                        | true  -> replacePattern item                                  
                                        | false -> item                                                                  

                                    //s chybnymi udaji v datech uz nic nenadelam, bez komplikovanych reseni..., tohle selekce vyradi jako neplatne (v JR je 2023_12_31)
                                    //https://kodis-files.s3.eu-central-1.amazonaws.com/NAD_2022_12_11_2023_03_31_v_1a2f33dafa.pdf
                                    //https://kodis-files.s3.eu-central-1.amazonaws.com/55_2023_07_26_2023_09_03_v_6186b834e8.pdf

                                    let fileName =  
                                        match item.Contains @"timetables/" with
                                        | true  -> item.Replace(pathKodisAmazonLink, String.Empty).Replace("timetables/", String.Empty).Replace(".pdf", "_t.pdf")
                                        | false -> item.Replace(pathKodisAmazonLink, String.Empty)  
                                                    
                                    let charList = 
                                        match fileName |> String.length >= lineNumberLength with  
                                        | true  -> fileName.ToCharArray() |> Array.toList |> List.take lineNumberLength
                                        | false -> 
                                                    message.msg9() 
                                                    []
                                             
                                    let a i range = range |> List.filter (fun item -> (charList |> List.item i = item)) 
                                    let b range = range |> List.contains (fileName.Substring(0, 3))

                                    let fileNameFullA = 
                                        MyBuilder
                                            {
                                                let!_ = not <| fileName.Contains("NAD"), fileName 
                                                let!_ = (<>) (a 0 range) [], fileName 
                                                let!_ = (<>) (a 1 range) [], sprintf "%s%s" "00" fileName //pocet "0" zavisi na delce retezce cisla linky 
                                                let!_ = (<>) (a 2 range) [], sprintf "%s%s" "0" fileName 

                                                return fileName
                                            }                                                            
                                                         
                                    let fileNameFull =  
                                        match b rangeS || b rangeR || b rangeX1 || b rangeA with
                                        | true  -> sprintf "%s%s" "_" fileNameFullA                                                                       
                                        | false -> fileNameFullA  

                                    let numberOfChar =  //vyhovuje i pro NAD
                                        match fileNameFull.Contains("_v") || fileNameFull.Contains("_t") with
                                        | true  -> 27  //27 -> 113_2022_12_11_2023_12_09_t......   //overovat, jestli se v jsonu nezmenila struktura nazvu                                                                
                                        | false -> 25  //25 -> 113_2022_12_11_2023_12_09......
                                                                                                                                                                                                                   
                                    match not (fileNameFull |> String.length >= numberOfChar) with 
                                    | true  -> String.Empty
                                    | false ->                                                                        
                                                let yearValidityStart x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(4 + x, 4) 
                                                let monthValidityStart x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(9 + x, 2) 
                                                let dayValidityStart x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(12 + x, 2) 
                                                                   
                                                let yearValidityEnd x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(15 + x, 4) 
                                                let monthValidityEnd x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(20 + x, 2) 
                                                let dayValidityEnd x = parseMeInt <| message.msgParam10 <| fileNameFull <| fileNameFull.Substring(23 + x, 2) 
                                                                   
                                                let a x =
                                                    [ 
                                                        yearValidityStart x
                                                        monthValidityStart x
                                                        dayValidityStart x
                                                        yearValidityEnd x
                                                        monthValidityEnd x
                                                        dayValidityEnd x
                                                    ]
                                                                   
                                                let result x = 

                                                    match (a x) |> List.contains -1 with
                                                    | true  -> 
                                                            let cond = 
                                                                match param with 
                                                                | CurrentValidity           -> true //s tim nic nezrobim, nekonzistentni informace v retezci
                                                                | FutureValidity            -> true //s tim nic nezrobim, nekonzistentni informace v retezci
                                                                | ReplacementService        -> 
                                                                                                fileNameFull.Contains("_v") 
                                                                                                || fileNameFull.Contains("X")
                                                                                                || fileNameFull.Contains("NAD")
                                                                | WithoutReplacementService -> 
                                                                                                not <| fileNameFull.Contains("_v") 
                                                                                                && not <| fileNameFull.Contains("X")
                                                                                                && not <| fileNameFull.Contains("NAD")

                                                            match cond with
                                                            | true  -> fileNameFull
                                                            | false -> String.Empty 
                                                                              
                                                    | false -> 
                                                            try                                                                                  
                                                                let dateValidityStart x = new DateTime(yearValidityStart x, monthValidityStart x, dayValidityStart x)                                                                                       
                                                                let dateValidityEnd x = new DateTime(yearValidityEnd x, monthValidityEnd x, dayValidityEnd x) 
                                                                                
                                                                let cond = 
                                                                    match param with 
                                                                    | CurrentValidity           -> 
                                                                                                    (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime 
                                                                                                    && 
                                                                                                    dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                    ||
                                                                                                    ((dateValidityStart x).Equals(currentTime) 
                                                                                                    && 
                                                                                                    (dateValidityEnd x).Equals(currentTime))

                                                                    | FutureValidity            -> dateValidityStart x |> Fugit.isAfter currentTime

                                                                    | ReplacementService        -> 
                                                                                                    (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime
                                                                                                    && 
                                                                                                    dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                    &&
                                                                                                    (fileNameFull.Contains("_v") 
                                                                                                    || fileNameFull.Contains("X")
                                                                                                    || fileNameFull.Contains("NAD"))

                                                                    | WithoutReplacementService ->
                                                                                                    (dateValidityStart x |> Fugit.isBeforeOrEqual currentTime
                                                                                                    && 
                                                                                                    dateValidityEnd x |> Fugit.isAfterOrEqual currentTime)
                                                                                                    &&
                                                                                                    (not <| fileNameFull.Contains("_v") 
                                                                                                    && not <| fileNameFull.Contains("X")
                                                                                                    && not <| fileNameFull.Contains("NAD"))
                                                                                
                                                                match cond with
                                                                | true  -> fileNameFull
                                                                | false -> String.Empty                                                                                
                                                                               
                                                            with 
                                                            | _ -> String.Empty  

                                                let condNAD (rangeN: string list) =                                                                     
                                                    rangeN
                                                    |> List.tryFind (fun item -> fileNameFull.Contains(item))                                                                                    
                                                    |> Option.isSome   
                                                                               
                                                let condNAD = xor (condNAD rangeN1) (condNAD rangeN2) 
                                                                                
                                                let x = //int hodnota je korekce pozice znaku v retezci
                                                    MyBuilder
                                                        {
                                                            let!_ = not (fileNameFull.Contains("NAD") && condNAD = true), 2
                                                            let!_ = not (List.exists (fun item -> fileNameFull.Contains(item: string)) rangeX2), 1
                                                            return 0
                                                        }
                                                                       
                                                result x
                                                   
                ) |> Array.toList |> List.distinct 

        //tryWith myFunction (fun x -> ()) () 0 [] |> deconstructor message.msgParam1
        tryWith myFunction (fun x -> ()) () String.Empty [] |> deconstructor message.msgParam1
    
    let myList1 = 
        myList |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))     
    
    //****************druha filtrace odkazu na neplatne jizdni rady***********************
   
    let myList2 = 
        let myFunction x = 
            //list listu se stejnymi linkami s ruznou dobou platnosti JR  
            myList1 
            |> splitListByPrefix message  //splitList1 //splitList 
            |> List.collect
                (fun list ->  
                            match (>) (list |> List.length) 1 with 
                            | false -> list 
                            | true  -> 
                                       let latestValidityStart =  
                                           list
                                           |> List.map
                                               (fun item -> 
                                                          let item = string item                                                                              
                                                          try
                                                              let condNAD (rangeN: string list) =                                                                     
                                                                  rangeN
                                                                  |> List.tryFind (fun item1 -> item.Contains(item1))                                                                                    
                                                                  |> Option.isSome                                                                                           
                                                                                              
                                                              let condNAD = xor (condNAD rangeN1) (condNAD rangeN2) 
                                                                               
                                                              let x = //int hodnota je korekce pozice znaku v retezci
                                                                  MyBuilder
                                                                      {
                                                                          let!_ = not (item.Contains("NAD") && condNAD = true), 2
                                                                          let!_ = not (List.exists (fun item1 -> item.Contains(item1: string)) rangeX2), 1
                                                                          return 0
                                                                      } 
                                                                                      
                                                              let yearValidityStart x = parseMeInt <| message.msgParam10 <| item <| item.Substring(4 + x, 4) //overovat, jestli se v jsonu neco nezmenilo //113_2022_12_11_2023_12_09.....
                                                              let monthValidityStart x = parseMeInt <| message.msgParam10 <| item <| item.Substring(9 + x, 2) 
                                                              let dayValidityStart x = parseMeInt <| message.msgParam10 <| item <| item.Substring(12 + x, 2)
                                                              
                                                              let yearValidityEnd x = parseMeInt <| message.msgParam10 <| item <| item.Substring(15 + x, 4) 
                                                              let monthValidityEnd x = parseMeInt <| message.msgParam10 <| item <| item.Substring(20 + x, 2) 
                                                              let dayValidityEnd x = parseMeInt <| message.msgParam10 <| item <| item.Substring(23 + x, 2) 
                                                              item, new DateTime(yearValidityStart x, monthValidityStart x, dayValidityStart x) 
                                                                //item, new DateTime(yearValidityEnd x, monthValidityEnd x, dayValidityEnd x) //pro pripadnou zmenu logiky
                                                          with 
                                                          | _ -> item, currentTime
                                               ) |> List.maxBy snd                                                        
                                       [ fst latestValidityStart ]                                                   
                ) |> List.distinct                              
        
        tryWith myFunction (fun x -> ()) () String.Empty [] |> deconstructor message.msgParam1
        
    let myList3 = 
        myList2 |> List.filter (fun item -> not <| String.IsNullOrWhiteSpace(item) && not <| String.IsNullOrEmpty(item))
  
    let myList4 = 
        let myFunction x = 
            myList3 
            |> List.map 
                (fun (item: string) ->     
                                    let item = string item   
                                    let str = item
                                    let str =
                                        match str.Substring(0, 2).Equals("00") with
                                        | true  ->
                                                str.Remove(0, 2)
                                        | false ->
                                                match str.Substring(0, 1).Equals("0") || str.Substring(0, 1).Equals("_") with
                                                | false -> item
                                                | true  -> str.Remove(0, 1)                                                                                  
                                             
                                    let link = 
                                        match item.Contains("_t") with 
                                        | true  -> (sprintf "%s%s%s" pathKodisAmazonLink @"timetables/" str).Replace("_t", String.Empty)
                                        | false -> sprintf "%s%s" pathKodisAmazonLink str                                                

                                    let path =     
                                        match item.Contains("_t") with 
                                        | true  -> 
                                                let fileName = item.Substring(0, item.Length) //zatim bez generovaneho kodu, sem tam to zkontrolovat
                                                sprintf "%s/%s" pathToDir fileName   
                                        | false -> 
                                                let fileName = item.Substring(0, item.Length - 15) //bez 15 znaku s generovanym kodem a priponou pdf dostaneme toto: 113_2022_12_11_2023_12_09 
                                                sprintf "%s/%s%s" pathToDir fileName ".pdf"  //pdf opet musime pridat
                                                           
                                    link, path 
            )
        
        tryWith myFunction (fun x -> ()) () String.Empty [] |> deconstructor message.msgParam1   
    
    myList4 
    |> List.filter
        (fun item -> 
                  (not <| String.IsNullOrWhiteSpace(fst item) 
                  && 
                  not <| String.IsNullOrEmpty(fst item)) 
                  ||
                  (not <| String.IsNullOrWhiteSpace(snd item)
                  && 
                  not <| String.IsNullOrEmpty(snd item))                                         
        ) |> List.sort                                             
        
let internal deleteAllODISDirectories message pathToDir = 

    let myDeleteFunction x =   

        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
        let dirInfo = new DirectoryInfo(pathToDir) |> Option.toGenerics (lazy (message.msgParam7 "Error8")) (new DirectoryInfo(pathToDir))             
       
        //smazeme pouze adresare obsahujici stare JR, ostatni ponechame   
        let deleteIt = 
            dirInfo.EnumerateDirectories()
            |> Option.toGenerics (lazy (message.msgParam7 "Chyba v průběhu odstraňování starých JŘ KODIS.")) Seq.empty  
            |> Array.ofSeq
            |> Array.filter (fun item -> (getDefaultRecordValues |> List.contains item.Name)) //prunik dvou kolekci (plus jeste Array.distinct pro unique items)
            |> Array.distinct 
            |> Array.Parallel.iter (fun item -> item.Delete(true))     
        deleteIt 
        
    tryWith myDeleteFunction (fun x -> ()) () String.Empty () |> deconstructor message.msgParam1

    message.msg10() 
    message.msg11() 
 
let internal createNewDirectories pathToDir = getDefaultRecordValues |> List.map (fun item -> sprintf"%s\%s"pathToDir item)  

let internal createDirName variant =
    match variant with 
    | CurrentValidity           -> getDefaultRecordValues |> List.item 0
    | FutureValidity            -> getDefaultRecordValues |> List.item 1
    | ReplacementService        -> getDefaultRecordValues |> List.item 2                                
    | WithoutReplacementService -> getDefaultRecordValues |> List.item 3

let internal deleteOneODISDirectory message variant pathToDir =

    //smazeme pouze jeden adresar obsahujici stare JR, ostatni ponechame

    let myDeleteFunction x = 

        //rozdil mezi Directory a DirectoryInfo viz Unique_Identifier_And_Metadata_File_Creator.sln -> MainLogicDG.fs
        let dirInfo = new DirectoryInfo(pathToDir) |> Option.toGenerics (lazy (message.msgParam7 "Chyba v průběhu odstraňování starých JŘ KODIS.")) (new DirectoryInfo(pathToDir))        
       
        dirInfo.EnumerateDirectories()
        |> Option.toGenerics (lazy (message.msgParam7 "Chyba v průběhu odstraňování starých JŘ KODIS.")) Seq.empty  
        |> Seq.filter (fun item -> item.Name = createDirName variant) 
        |> Seq.iter (fun item -> item.Delete(true)) //trochu je to hack, ale nemusim se zabyvat tryHead, bo moze byt empty kolekce
                  
    tryWith myDeleteFunction (fun x -> ()) () String.Empty () |> deconstructor message.msgParam1

    message.msg10() 
    message.msg11()        

let internal createOneNewDirectory pathToDir dirName = [ sprintf"%s\%s"pathToDir dirName ] //list -> aby bylo mozno pouzit funkci createFolders bez uprav  

let internal createFolders message dirList =  

   let myFolderCreation x = 
       dirList |> List.iter (fun dir -> Directory.CreateDirectory(dir) |> ignore)  
              
   tryWith myFolderCreation (fun x -> ()) () String.Empty () |> deconstructor message.msgParam1   

let internal downloadAndSaveTimetables (client: Http.HttpClient) message pathToDir (filterTimetables: (string*string) list)  = 

    let downloadFileTaskAsync (uri: string) (path: string) =   

            async
                {   //muj custom-made tryWith nezachyti exception u async
                    //info about the complexity of concurrent downloading https://stackoverflow.com/questions/6219726/throttled-async-download-in-f
                    try 
                        let! stream = client.GetStreamAsync(uri) |> Async.AwaitTask                             
                        use fileStream = new FileStream(path, FileMode.CreateNew) //|> (optionToGenerics "Error9" (new FileStream(path, FileMode.CreateNew))) //nelze, vytvari to dalsi stream a uklada to znovu                                
                        return! stream.CopyToAsync(fileStream) |> Async.AwaitTask                        
                    with 
                    | :? AggregateException as ex -> 
                                                     message.msgParam2 uri 
                                                     return()                                              
                    | ex                          -> 
                                                     //deconstructorError <| message.msgParam1 (string ex) <| client.Dispose()
                                                     deconstructorError <| message.msgParam1 "Chyba v průběhu stahování JŘ KODIS." <| client.Dispose()
                                                     return()                                
                }     

    //tryWith je ve funkci downloadFileTaskAsync
    message.msgParam3 pathToDir 
        
    let downloadTimetables filterTimetables = 

        let l = filterTimetables |> List.length
        filterTimetables 
        |> List.iteri 
            (fun i (link, pathToFile) -> //Array.Parallel.iter tady nelze  
                                         //async { printfn"%s" pathToFile; return! Async.Sleep 0 } //for testing
                                        async                                                 
                                            {
                                                progressBarContinuous message i l
                                                return! downloadFileTaskAsync link pathToFile 
                                            }
                                            |> Async.Catch
                                            |> Async.RunSynchronously
                                            |> Result.ofChoice
                                            |> Result.toOption
                                            |> function
                                                | Some value -> ()                                                                                 
                                                | None       -> message.msgParam2 link                                                         
            )                           
   
    downloadTimetables filterTimetables 
    
    message.msgParam4 pathToDir 

let internal downloadAndSave message variant dir client = 

    match dir |> Directory.Exists with 
    | false -> 
             message.msgParam5 dir 
             message.msg13()                                                
    | true  ->                  
             digThroughJsonStructure message 
             |> filterTimetables message variant dir 
             |> downloadAndSaveTimetables client message dir  