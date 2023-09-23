module ProgressBarFSharp

open System
open System.Threading

open Messages.Messages
//open Messages.MessagesMocking

open ErrorHandling
open ErrorHandling.TryWith

//TODO pojmenovat ErrorPB4 atd. nejak lepe  :-)

let private (++) a = (+) a 1

let inline private updateProgressBar (message: Messages) (currentProgress : int) (totalProgress : int) : unit =
    
    let myFunction x = 

        let bytes = //437 je tzv. Extended ASCII  
            System.Text.Encoding.GetEncoding(437).GetBytes("█") |> Option.toSrtp (lazy (message.msgParam7 "Indikátor průběhu má problém, který ale neovlivní stahování JŘ.")) [||] 
                   
        let output =
            System.Text.Encoding.GetEncoding(852).GetChars(bytes) |> Option.toSrtp (lazy (message.msgParam7 "Indikátor průběhu má problém, který ale neovlivní stahování JŘ.")) [||]   
        
        let progressBar = 
            let barWidth = 50 //nastavit delku dle potreby            
            let percentComplete = (/) ((*) currentProgress 101) ((++) totalProgress) // :-) //101 proto, ze pri deleni 100 to po zaokrouhleni dalo jen 99%                    
            let barFill = (/) ((*) currentProgress barWidth) totalProgress // :-)  
               
            let characterToFill = string (Array.item 0 output) //moze byt baj "#"
            let bar = String.replicate barFill characterToFill |> Option.toSrtp (lazy (message.msgParam7 "Indikátor průběhu má problém, který ale neovlivní stahování JŘ.")) String.Empty 
            let remaining = String.replicate (barWidth - (++) barFill) "*" |> Option.toSrtp (lazy (message.msgParam7 "Indikátor průběhu má problém, který ale neovlivní stahování JŘ.")) String.Empty // :-)
              
            sprintf "<%s%s> %d%%" bar remaining percentComplete 

        match (=) currentProgress totalProgress with
        | true  -> message.msgParam8 progressBar
        | false -> message.msgParam9 progressBar

    tryWith myFunction (fun x -> ()) () String.Empty () |> deconstructor Messages.Default.msgParam1

let internal progressBarContinuous (message: Messages) (currentProgress : int) (totalProgress : int) : unit =

    match currentProgress < (totalProgress - 1) with
    | true  -> updateProgressBar message currentProgress totalProgress
    | false -> 
               Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r")
               Console.CursorLeft <- 0             