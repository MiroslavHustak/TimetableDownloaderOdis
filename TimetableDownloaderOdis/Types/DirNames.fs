namespace Types

module DirNames =

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
                odisDir4 = "JR_ODIS_teoreticky_dlouhodobe_platne_bez_vyluk" 
                odisDir5 = "JR_ODIS_pouze_linky_dopravce_DPO" 
                odisDir6 = "JR_ODIS_pouze_linky_dopravce_MDPO" 
            }   


