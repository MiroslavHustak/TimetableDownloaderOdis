namespace Parsing

module TryParserInt =

     let tryParseWith (tryParseFunc: string -> bool * _) =
         tryParseFunc >> function
         | true, value -> Some value
         | false, _    -> None
     let parseInt = tryParseWith <| System.Int32.TryParse  
     let (|Int|_|) = parseInt  
     
module TryParserDate = //tohle je pro parsing textoveho retezce do DateTime, ne pro overovani new DateTime()

       let tryParseWith (tryParseFunc: string -> bool * _) =
           tryParseFunc >> function
           | true, value -> Some value
           | false, _    -> None
       let parseDate = tryParseWith <| System.DateTime.TryParse 
       let (|Date|_|) = parseDate                 
                                    
//**************************************************************************************************                                  
//Toto neni pouzivany kod, ale jen pattern pro tvorbu TryParserInt, TryParserDate atd. Neautorsky kod.
module private TryParser =

     let tryParseWith (tryParseFunc: string -> bool * _) = 
         tryParseFunc >> function
                         | true, value -> Some value
                         | false, _    -> None

     let parseDate   = tryParseWith <| System.DateTime.TryParse
     let parseInt    = tryParseWith <| System.Int32.TryParse
     let parseSingle = tryParseWith <| System.Single.TryParse
     let parseDouble = tryParseWith <| System.Double.TryParse
     // etc.

     // active patterns for try-parsing strings
     let (|Date|_|)   = parseDate
     let (|Int|_|)    = parseInt
     let (|Single|_|) = parseSingle
     let (|Double|_|) = parseDouble


