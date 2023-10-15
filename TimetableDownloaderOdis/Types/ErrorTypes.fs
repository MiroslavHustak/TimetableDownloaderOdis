namespace Types

module ErrorTypes =

    open System

    type ConnErrorCode = //reflection nefunguje s type internal
        {
            BadRequest: string
            InternalServerError: string
            NotImplemented: string
            ServiceUnavailable: string        
            NotFound: string
            CofeeMakerUnavailable: string
        }
        static member Default =                 
            {
                BadRequest            = "400 Bad Request"
                InternalServerError   = "500 Internal Server Error"
                NotImplemented        = "501 Not Implemented"
                ServiceUnavailable    = "503 Service Unavailable"           
                NotFound              = String.Empty  
                CofeeMakerUnavailable = "418 I'm a teapot. Look for a coffee maker elsewhere."
            }   

