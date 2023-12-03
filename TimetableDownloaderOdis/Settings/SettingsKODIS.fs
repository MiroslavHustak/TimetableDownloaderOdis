module SettingsKODIS

open System

open Fugit

//************************Constants and types**********************************************************************

//tu a tam zkontrolovat json, zdali KODIS nezmenil jeho strukturu 
//pro type provider musi byt konstanta (nemozu pouzit sprintf partialPathJson) a musi byt forward slash"

//Tohle nefunguje s json type provider
let internal pathJsonNotWorkingForTypeProviders = 
    try
        let path = AppDomain.CurrentDomain.BaseDirectory + "KODISJson" + @"/kodisMHDTotal.json" //Copy Always     
        path
    with
    | ex -> failwith (sprintf "Some mysterious exception: %s" ex.Message)  

let [<Literal>] internal pathJson = @"KODISJson/kodisMHDTotal.json" //v hl. adresari projektu

let [<Literal>] internal partialPathJson = @"KODISJson/" //v binu //tohle je pro stahovane json, ne pro type provider

let [<Literal>] internal pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"
let [<Literal>] internal pathKodisAmazonLink = @"https://kodis-files.s3.eu-central-1.amazonaws.com/" 
let [<Literal>] internal nonJsonString = "12345 #$%&*()"

let internal currentTime = Fugit.today() //nelze Fugit.now //.AddDays(-1.0)   // new DateTime(2023, 04, 11)
let internal regularValidityStart = new DateTime(2022, 12, 11) //zmenit pri pravidelne zmene JR 
let internal regularValidityEnd = new DateTime(2023, 12, 09) //zmenit pri pravidelne zmene JR 
//let internal regularValidityStart = new DateTime(2023, 12, 10) //zmenit pri pravidelne zmene JR 
//let internal regularValidityEnd = new DateTime(2024, 12, 14) //zmenit pri pravidelne zmene JR 

let internal range = [ 0..9 ] |> List.map (fun item -> char (int '0' + item))  //[ '1'; '2'; .... ] //nema souvislost s nize uvedenym
//int '0': Converts the character '0' to its Unicode code point, which is equivalent to the integer 48.
//int '0' + item: Adds the current integer item to 48, effectively converting it to the ASCII value of the corresponding digit character.
//For example, if item is 3, this part results in 51, which corresponds to the ASCII value of the character '3'.

let internal rangeS = [ 1..9 ] |> List.map (fun item -> sprintf "S%s_" <| string item)  //[ "S1_"; "S2_"; ..... ]
let internal rangeR = [ 1..9 ] |> List.map (fun item -> sprintf "R%s_" <| string item)  //[ "R1_"; "R2_"; ..... ]

let internal rangeS1 = [ 10..99 ] |> List.map (fun item -> sprintf "S%s_" <| string item) //trimistne, jen pro tvorbu rangeNS1
let internal rangeR1 = [ 10..99 ] |> List.map (fun item -> sprintf "R%s_" <| string item)  //trimistne, jen pro tvorbu rangeNR1

let internal rangeX = [ 1..9 ] |> List.map (fun item -> sprintf "X%s_" <| string item)  //[ "X1_"; "X2_"; ..... ]
//let internal rangeX1 = [ 10..99 ] |> List.map (fun item -> sprintf "X%s_" <| string item) //trimistne
let internal rangeX2_1 = [ 100..119 ] |> List.map (fun item -> sprintf "X%s_" <| string item) //DPO
let internal rangeX2_2 = [ 201..229 ] |> List.map (fun item -> sprintf "X%s_" <| string item) //MDPO [ "X201_"; "X202_"; ..... ]
let internal rangeX2 = List.append rangeX2_1 rangeX2_2

let internal rangeA = [ "AE_" ] //ponechan prostor pro pripadne cislovani AE

let internal rangeN1 = [ 1..9 ] |> List.map (fun item -> sprintf "NAD_%s_" <| string item) //[ "NAD_1_"; "NAD_2_"; ........... ]
let internal rangeN2 = [ 10..19 ] |> List.map (fun item -> sprintf "NAD_%s_" <| string item) //[ "NAD_10_"; "NAD_11_"; ....... ]
let internal rangeN3 = [ 230..999 ] |> List.map (fun item -> sprintf "NAD_%s_" <| string item)

let internal rangeNS = rangeS |> List.map (fun item -> sprintf "NAD_%s_" <| string item)
let internal rangeNR = rangeR |> List.map (fun item -> sprintf "NAD_%s_" <| string item) 
let internal rangeNS1 = rangeS1 |> List.map (fun item -> sprintf "NAD_%s_" <| string item) 
let internal rangeNR1 = rangeR1 |> List.map (fun item -> sprintf "NAD_%s_" <| string item)

let [<Literal>] internal lineNumberLength = 3 //3 je delka retezce pouze pro linky 001 az 999

let internal jsonLinkList = //pri zmene jsonu na strankach KODISu zmenit aji nazev souboru, napr. kodisRegion3001.json

    [
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=MHD%20Bruntál&group_in%5B1%5D=MHD%20Český%20Těšín&group_in%5B2%5D=MHD%20Frýdek-Místek&group_in%5B3%5D=MHD%20Havířov&group_in%5B4%5D=MHD%20Karviná&group_in%5B5%5D=MHD%20Krnov&group_in%5B6%5D=MHD%20Nový%20Jičín&group_in%5B7%5D=MHD%20Opava&group_in%5B8%5D=MHD%20Orlová&group_in%5B9%5D=MHD%20Ostrava&group_in%5B10%5D=MHD%20Studénka&group_in%5B11%5D=MHD%20Třinec&group_in%5B12%5D=NAD%20MHD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Bruntál&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Český%20Těšín&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Frýdek-Místek&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Havířov&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Karviná&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Krnov&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Nový%20Jičín&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Opava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Orlová&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=24&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=36&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label" 
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=48&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=60&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=72&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label" 
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=84&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Studénka&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=MHD%20Třinec&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD%20MHD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&group_in%5B1%5D=232-293&group_in%5B2%5D=331-392&group_in%5B3%5D=440-465&group_in%5B4%5D=531-583&group_in%5B5%5D=613-699&group_in%5B6%5D=731-788&group_in%5B7%5D=811-885&group_in%5B8%5D=901-990&group_in%5B9%5D=NAD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=75&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=232-293&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=331-392&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=440-465&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=531-583&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=613-699&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=731-788&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=811-885&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=901-990&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=12&group_in%5B0%5D=S1-S34&group_in%5B1%5D=R8-R61&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=S1-S34&_sort=numeric_label"
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=R8-R61&_sort=numeric_label"   
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=0&group_in%5B0%5D=NAD&_sort=numeric_label" 
    ]

let internal pathToJsonList = 
    
    [
        sprintf "%s%s" partialPathJson @"kodisMHDTotal1.json"
        sprintf "%s%s" partialPathJson @"kodisMHDBruntal.json"
        sprintf "%s%s" partialPathJson @"kodisMHDCT.json"
        sprintf "%s%s" partialPathJson @"kodisMHDFM.json"
        sprintf "%s%s" partialPathJson @"kodisMHDHavirov.json"
        sprintf "%s%s" partialPathJson @"kodisMHDKarvina.json"
        sprintf "%s%s" partialPathJson @"kodisMHDBKrnov.json"
        sprintf "%s%s" partialPathJson @"kodisMHDNJ.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOpava.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOrlova.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava0.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava1.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava2.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava3.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava4.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava5.json"
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava6.json"
        sprintf "%s%s" partialPathJson @"kodisMHDStudenka.json"
        sprintf "%s%s" partialPathJson @"kodisMHDTrinec.json"
        sprintf "%s%s" partialPathJson @"kodisMHDNAD.json"
        sprintf "%s%s" partialPathJson @"kodisRegionTotal.json"
        sprintf "%s%s" partialPathJson @"kodisRegion75.json"
        sprintf "%s%s" partialPathJson @"kodisRegion200.json"
        sprintf "%s%s" partialPathJson @"kodisRegion3001.json"
        sprintf "%s%s" partialPathJson @"kodisRegion400.json"
        sprintf "%s%s" partialPathJson @"kodisRegion500.json"
        sprintf "%s%s" partialPathJson @"kodisRegion600.json"
        sprintf "%s%s" partialPathJson @"kodisRegion700.json"
        sprintf "%s%s" partialPathJson @"kodisRegion800.json"
        sprintf "%s%s" partialPathJson @"kodisRegion900.json"
        sprintf "%s%s" partialPathJson @"kodisRegionNAD.json"
        sprintf "%s%s" partialPathJson @"kodisTrainTotal1.json"
        sprintf "%s%s" partialPathJson @"kodisTrainPomaliky.json"
        sprintf "%s%s" partialPathJson @"kodisTrainSpesakyARychliky.json"   
        sprintf "%s%s" partialPathJson @"kodisNAD.json"
    ]