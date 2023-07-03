module SettingsKODIS

open System

open Fugit

[<Struct>]  //vhodne pro 16 bytes => 4096 characters
type ODIS = 
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

let [<Literal>] pathJson = @"KODISJson/kodisMHDTotal.json" //v hl. adresari projektu

let [<Literal>] partialPathJson = @"KODISJson/" //v binu

let [<Literal>] pathKodisWeb = @"https://kodisweb-backend.herokuapp.com/"
let [<Literal>] pathKodisAmazonLink = @"https://kodis-files.s3.eu-central-1.amazonaws.com/"

let currentTime = Fugit.now()//.AddDays(-1.0)   // new DateTime(2023, 04, 11)
let regularValidityStart = new DateTime(2022, 12, 11) //zmenit pri pravidelne zmene JR 
let regularValidityEnd = new DateTime(2023, 12, 09) //zmenit pri pravidelne zmene JR 
let range = [ '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; '0' ]
let rangeS = [ "S1_"; "S2_"; "S3_"; "S4_"; "S5_"; "S6_"; "S7_"; "S8_"; "S9_" ]
let rangeR = [ "R1_"; "R2_"; "R3_"; "R4_"; "R5_"; "R6_"; "R7_"; "R8_"; "R9_" ]
let rangeX = [ "X1_"; "X2_"; "X3_"; "X4_"; "X5_"; "X6_"; "X7_"; "X8_"; "X9_" ]
let rangeA = [ "AE_" ] //ponechan prostor pro pripadne cislovani AE
let rangeN1 = [ "NAD_1_"; "NAD_2_"; "NAD_3_"; "NAD_4_"; "NAD_5_"; "NAD_6_"; "NAD_7_"; "NAD_8_"; "NAD_9_" ]
let rangeN2 = [ "NAD_10_"; "NAD_11_"; "NAD_12_"; "NAD_13_"; "NAD_14_"; "NAD_15_"; "NAD_16_"; "NAD_17_"; "NAD_18_"; "NAD_19_" ]
//TODO jestli bude cas, pridelat NAD vlakovych linek

let [<Literal>] lineNumberLength = 3 //3 je delka retezce pouze pro linky 001 az 999