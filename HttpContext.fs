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
        Headers:string List;
    }

type RequestData =
  // first list is url substrings, second list is query string pairs
  | RequestData of string list * (string * string) list

type ResponseData =
  | Json of string

type HandlerFunction = (RequestData -> ResponseData)
type DispatchEntry = (string * HandlerFunction)
type DispatchEntryEx = (Regex * HandlerFunction)

let ParseRequest rawData = 
    