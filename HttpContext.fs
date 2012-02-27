module Mario.HttpContext

open System.Text.RegularExpressions

type HttpMethod = 
    | GET 
    | POST
    | PUT
    | DELETE
   
type HttpRequest =
    {
        Url:string;
        Query:string;
        Method:HttpMethod;
        Headers:seq<string>;
    }

type RequestData =
  // first list is url substrings, second list is query string pairs
  | RequestData of string list * (string * string) list

type ResponseData =
  | Json of string

type HandlerFunction = (RequestData -> ResponseData)
type DispatchEntry = (string * HandlerFunction)
type DispatchEntryEx = (Regex * HandlerFunction)


let split (sep:string) (str:string) =
    match sep, str with
    | ((null | ""), _) | (_, (null | "")) -> seq [str]
    | _ ->
        let n, j = str.Length, sep.Length
        let rec loop p = 
            seq {
                if p < n then
                    let i = match str.IndexOf(sep, p) with -1 -> n | i -> i
                    yield str.Substring(p, i - p)
                    yield! loop (i + j)
            }
        loop 0


let ParseRequest rawData = 
    let seqHeaders = split "\r\n" rawData
    let mainHeader = Seq.nth 1 seqHeaders     
    match mainHeader with
        | (null | "") -> "no header"
        | _ -> "something exist"
    