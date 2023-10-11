module SettingsKODIS

open System

open Fugit

[<Struct>]   //vhodne pro 16 bytes => 4096 characters
type ODIS =  //reflection nefunguje s type internal
    {        
        odisDir1: string
        odisDir2: string
        odisDir3: string
        odisDir4: string
        odisDir5: string
        odisDir6: string
    }
    static member Default = 
        {          
            odisDir1 = "JR_ODIS_aktualni_vcetne_vyluk"
            odisDir2 = "JR_ODIS_pouze_budouci_platnost"
            odisDir3 = "JR_ODIS_pouze_vyluky"
            odisDir4 = "JR_ODIS_kompletni_bez_vyluk" 
            odisDir5 = "JR_ODIS_pouze_linky_dopravce_DPO" 
            odisDir6 = "JR_ODIS_pouze_linky_dopravce_MDPO" 
        }   

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

let internal currentTime = Fugit.now()//.AddDays(-1.0)   // new DateTime(2023, 04, 11)
let internal regularValidityStart = new DateTime(2022, 12, 11) //zmenit pri pravidelne zmene JR 
let internal regularValidityEnd = new DateTime(2023, 12, 09) //zmenit pri pravidelne zmene JR 
let internal range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
let internal rangeS = [ "S1_"; "S2_"; "S3_"; "S4_"; "S5_"; "S6_"; "S7_"; "S8_"; "S9_" ]
let internal rangeR = [ "R1_"; "R2_"; "R3_"; "R4_"; "R5_"; "R6_"; "R7_"; "R8_"; "R9_" ]
let internal rangeX1 = [ "X1_"; "X2_"; "X3_"; "X4_"; "X5_"; "X6_"; "X7_"; "X8_"; "X9_" ]
let internal rangeX2 = [ "X201_"; "X202_"; "X203_"; "X204_"; "X205_"; "X206_"; "X207_"; "X208_"; "X209_"; "X210_" ]
let internal rangeA = [ "AE_" ] //ponechan prostor pro pripadne cislovani AE
let internal rangeN1 = [ "NAD_1_"; "NAD_2_"; "NAD_3_"; "NAD_4_"; "NAD_5_"; "NAD_6_"; "NAD_7_"; "NAD_8_"; "NAD_9_" ]
let internal rangeN2 = [ "NAD_10_"; "NAD_11_"; "NAD_12_"; "NAD_13_"; "NAD_14_"; "NAD_15_"; "NAD_16_"; "NAD_17_"; "NAD_18_"; "NAD_19_" ]
//TODO jestli bude cas, pridelat NAD vlakovych linek

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
        sprintf "%s%s" pathKodisWeb @"linky?_limit=12&_start=72&group_in%5B0%5D=MHD%20Ostrava&_sort=numeric_label" 
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
        sprintf "%s%s" partialPathJson @"kodisMHDOstrava1.json"
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