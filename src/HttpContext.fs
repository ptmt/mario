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
    | Extension of string
    with
        member this.toString =
            match this:HttpMethod with
            | HttpMethod.GET -> "GET"
            | HttpMethod.POST -> "POST"
            | HttpMethod.DELETE -> "DELETE"
            | HttpMethod.PUT -> "PUT"
            | HttpMethod.CONNECT -> "CONNECT"
            | _ -> "not implemented" 
        static member fromString s =
            match s with
            | "GET" -> HttpMethod.GET
            | "POST" -> HttpMethod.POST
            | "PUT" -> HttpMethod.PUT
            | "DELETE" -> HttpMethod.DELETE
            | "CONNECT" -> HttpMethod.CONNECT
            | _ -> Extension "not implemented"

   
type HttpRequest =
    {
        Uri:string;
        Method:HttpMethod;
        Headers:string list;
        Body:string
    }   
type HttpResponse = 
    {
        Json : string
    }



let internal parseget<'a> : Parser<string, 'a> = pstring HttpMethod.GET.toString
let internal parsepost<'a> : Parser<string, 'a> = pstring HttpMethod.POST.toString
let internal parseput<'a> : Parser<string, 'a> = pstring HttpMethod.PUT.toString
let internal parsedelete<'a> : Parser<string, 'a> = pstring HttpMethod.DELETE.toString

let httpMethod<'a> : Parser<HttpMethod, 'a> =
  parseget <|> parsepost <|> parseput <|> parsedelete |>> HttpMethod.fromString

let notspaces: Parser<string, 'a> = 
    manySatisfy (function ' '|'\t'| '\n' -> false | _ -> true)

let noteol : Parser<string, 'a> =
    manySatisfy (function '\n' | '\r' -> false | _ ->true)

let httpRequestUri =
    spaces >>. notspaces

let httpHeaders: Parser<string list, 'a> =
    sepBy noteol newline

let httpRequest : Parser<HttpRequest, 'a> =
    pipe3 httpMethod httpRequestUri httpHeaders (fun x y z -> {Method = x; Uri = y; Headers = z; Body = z.Item (z.Length - 1)} )

let parse p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let ParseRequest (rawData:string) : HttpRequest option =
    match run httpRequest rawData with
    | Success(result, _, _)   -> 
        Some(result)
    | Failure(errorMsg, _, _) -> 
        printfn "Failure: %s" errorMsg
        None
