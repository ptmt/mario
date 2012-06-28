module Mario.HttpContext

// HTTP Request Protocol truncated implementation
// see http://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html

open System.Text.RegularExpressions
open FParsec


type HttpMethod = 
    | GET 
    | POST
    | PUT
    | DELETE
    | CONNECT
    | HEAD
    | TRACE 
    | OPTIONS
    | Extension of string
    with
        member this.toString =
            match this:HttpMethod with
            | HttpMethod.GET -> "GET"
            | HttpMethod.POST -> "POST"
            | HttpMethod.DELETE -> "DELETE"
            | HttpMethod.PUT -> "PUT"
            | HttpMethod.CONNECT -> "CONNECT"
            | HttpMethod.HEAD -> "HEAD"
            | HttpMethod.TRACE -> "TRACE"
            | HttpMethod.OPTIONS -> "OPTIONS"
            | _ -> "not implemented" 
        static member fromString s =
            match s with
            | "GET" -> HttpMethod.GET
            | "POST" -> HttpMethod.POST
            | "PUT" -> HttpMethod.PUT
            | "DELETE" -> HttpMethod.DELETE
            | "CONNECT" -> HttpMethod.CONNECT
            | "HEAD" -> HttpMethod.HEAD 
            | "TRACE" -> HttpMethod.TRACE 
            | "OPTIONS" -> HttpMethod.OPTIONS 
            | _ -> Extension "not implemented"

   
type HttpRequest =
    {
        Uri:string;
        Method:HttpMethod;
        Headers:string list;
        Body:string;
        SessionId:string;
    }   
type HttpResponse = 
    {
        Json : string
    }



let internal parseget<'a> : Parser<string, 'a> = pstring HttpMethod.GET.toString
let internal parsepost<'a> : Parser<string, 'a> = pstring HttpMethod.POST.toString
let internal parseput<'a> : Parser<string, 'a> = pstring HttpMethod.PUT.toString
let internal parsedelete<'a> : Parser<string, 'a> = pstring HttpMethod.DELETE.toString
let internal parseconnect<'a> : Parser<string, 'a> = pstring HttpMethod.CONNECT.toString 
let internal parsehead<'a> : Parser<string, 'a> = pstring HttpMethod.HEAD.toString 
let internal parsetrace<'a> : Parser<string, 'a> = pstring HttpMethod.TRACE.toString 
let internal parseoptions<'a> : Parser<string, 'a> = pstring HttpMethod.OPTIONS.toString 

let httpMethod<'a> : Parser<HttpMethod, 'a> =
  parseget <|> parsepost <|> parseput <|> parsedelete <|> parsehead <|> parseconnect <|> parsetrace <|> parseoptions |>> HttpMethod.fromString

let notspaces: Parser<string, 'a> = 
    manySatisfy (function ' '|'\t'| '\n' -> false | _ -> true)

let noteol : Parser<string, 'a> =
    manySatisfy (function '\n' | '\r' -> false | _ ->true)

let httpRequestUri =
    spaces >>. notspaces

let httpHeaders: Parser<string list, 'a> =
    sepBy noteol newline

let httpRequest : Parser<HttpRequest, 'a> =
    pipe3 httpMethod httpRequestUri httpHeaders (fun x y z -> {Method = x; Uri = y; Headers = z; Body = z.Item (z.Length - 1); SessionId = ""} )

let parse p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

//    let cookies = req.Value.Headers |> List.filter (fun x-> x.Contains("Cookie: SID=")) 
//                    if cookies.Length > 0 then 
//                        let c = cookies |> List.head 
//                        let sessionID = c.Replace("SID=", "")
//                    else
//                        let addHeaders = "\r\nSet-Cookie:SID=" + Mario.Session.genSessionId   
let ParseRequest (rawData:string) : HttpRequest option =
    match run httpRequest rawData with
    | Success(result, _, _)   -> 
        Some(result)
    | Failure(errorMsg, _, _) -> 
        //printfn "Failure: %s" errorMsg
        None
