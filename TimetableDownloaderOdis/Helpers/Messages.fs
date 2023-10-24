namespace Messages

open System

module Messages =

    type internal Messages = 
        {
            msg1: unit -> unit
            msg2: unit -> unit
            msg3: unit -> unit
            msg4: unit -> unit
            msg5: unit -> unit
            msg6: unit -> unit
            msg7: unit -> unit
            msg8: unit -> unit
            msg9: unit -> unit
            msg10: unit -> unit
            msg11: unit -> unit
            msg12: unit -> unit
            msg13: unit -> unit 
            msg14: unit -> unit

            msgParam1: string -> unit 
            msgParam2: string -> unit 
            msgParam3: string -> unit   
            msgParam4: string -> unit 
            msgParam5: string -> unit 
            msgParam6: string -> unit 
            msgParam7: string -> unit 
            msgParam8: string -> unit 
            msgParam9: string -> unit 
            msgParam10: string -> string -> unit 
            msgParam11: string -> unit 
            msgParam12: string -> unit 
        }
        static member Default = 
            {
                msg1 = fun () -> printfn "Zase se někdo vrtal v listu s odkazy a cestami. Je nutná jejich kontrola. Zmáčkni cokoliv pro ukončení programu." 
                msg2 = fun () -> printfn "Probíhá stahování a ukládání JSON souborů ze stránek KODISu."
                msg3 = fun () -> printfn "Dokončeno stahování a ukládání JSON souborů ze stránek KODISu."
                msg4 = fun () -> printfn "Probíhá filtrace odkazů na neplatné jízdní řády."
                msg5 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
                msg6 = fun () -> printfn "Chyba v průběhu stahování JSON souborů." 
                msg7 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
                msg8 = fun () -> printfn "Chyba v průběhu stahování JSON souborů."  
                msg9 = fun () -> printfn "Chyba při filtraci odkazů na JŘ." 
                msg10 = fun () -> printfn "Dokončena filtrace odkazů na neplatné jízdní řády."
                msg11 = fun () -> printfn "Provedeno odstranění všech starých JŘ ve zvolené variantě, pokud existovaly. \nChvíli strpení, prosím ..."
                msg12 = fun () -> printfn "Provedeno odstranění starých JŘ ve zvolené variantě, pokud existovaly. \nChvíli strpení, prosím ..."
                msg13 = fun () -> printfn "Pravděpodobně někdo odstranil daný adresář v průběhu práce tohoto programu."  
                msg14 = fun () -> printfn "Nadřel jsem se, ale úkol jsem úspěšně dokončil :-)"

                msgParam1 = printfn "\n%s%s" "No jéje, někde nastala chyba. Zmáčkni cokoliv pro ukončení programu a zkus to znovu. Popis chyby: " 
                msgParam2 = printfn "\n%s%s" "Jízdní řád s tímto odkazem se nepodařilo stáhnout: \n"  
                msgParam3 = printfn "Probíhá stahování příslušných JŘ a jejich ukládání do [%s]."  
                msgParam4 = printfn "Dokončeno stahování příslušných JŘ a jejich ukládání do [%s]. \nChvíli strpení, prosím..."  
                msgParam5 = printfn "Adresář [%s] neexistuje, příslušné JŘ do něj určené nemohly být staženy." 
                msgParam6 = printfn "Chyba v řetězci [%s]." 
                msgParam7 = printfn"%s" 
                msgParam8 = printfn "%s\n" 
                msgParam9 = printf "%s\r" 
                msgParam10 = printfn "Parsování neproběhlo korektně u této hodnoty: %s. Problém je u %s."  
                msgParam11 = printfn "Soubor %s nenalezen" 
                msgParam12 = printfn "Adresář %s nenalezen"
            }
   
module MessagesMocking =  

    type internal Messages = 
        {
            msg1: unit -> unit
            msg2: unit -> unit
            msg3: unit -> unit
            msg4: unit -> unit
            msg5: unit -> unit
            msg6: unit -> unit
            msg7: unit -> unit
            msg8: unit -> unit
            msg9: unit -> unit
            msg10: unit -> unit
            msg11: unit -> unit
            msg12: unit -> unit
            msg13: unit -> unit 
            msg14: unit -> unit

            msgParam1: string -> unit 
            msgParam2: string -> unit 
            msgParam3: string -> unit   
            msgParam4: string -> unit 
            msgParam5: string -> unit 
            msgParam6: string -> unit 
            msgParam7: string -> unit 
            msgParam8: string -> unit 
            msgParam9: string -> unit 
            msgParam10: string -> string -> unit 
            msgParam11: string -> unit 
            msgParam12: string -> unit
        }
        static member Default = 
            {
                msg1 = fun () -> () 
                msg2 = fun () -> ()
                msg3 = fun () -> ()
                msg4 = fun () -> ()
                msg5 = fun () -> ()
                msg6 = fun () -> ()
                msg7 = fun () -> ()
                msg8 = fun () -> ()
                msg9 = fun () -> ()
                msg10 = fun () -> ()
                msg11 = fun () -> ()
                msg12 = fun () -> ()
                msg13 = fun () -> ()  
                msg14 = fun () -> ()

                msgParam1 = fun (input: string) -> ()                     
                msgParam2 = fun (input: string) -> ()    
                msgParam3 = fun (input: string) -> ()    
                msgParam4 = fun (input: string) -> ()     
                msgParam5 = fun (input: string) -> ()   
                msgParam6 = fun (input: string) -> ()   
                msgParam7 = fun (input: string) -> ()   
                msgParam8 = fun (input: string) -> ()    
                msgParam9 = fun (input: string) -> ()   
                msgParam10 = fun (input1: string) (input2: string) -> () //fun (input: string) -> ()    
                msgParam11 = fun (input: string) -> ()  
                msgParam12 = fun (input: string) -> () 
            }
   