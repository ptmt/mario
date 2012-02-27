module Mario.Dispatcher

open System.Text.RegularExpressions
open System.Net
open System.Web
open System.Text
open Mario.HttpContext

let makeDispatcher (pattern, handler) =
  let fullPattern = sprintf "%s/?.*" pattern
  let rx = Regex(fullPattern, RegexOptions.Compiled)
  (rx, handler)
let prepareDispatchers (table:DispatchEntry list) : DispatchEntryEx list =
  List.map makeDispatcher table

//
//module Query =
//  let decode : string -> string =
//    HttpUtility.UrlDecode
//
//  let processParams (s:string) =
//    if String.IsNullOrEmpty(s) then
//      []
//    else
//      let s' = s.TrimStart([|'?'|])
//      s'.Split([|'&'|])
//      |> Array.map (fun pair -> pair.Split([|'='|]))
//      |> Array.map (fun [|h;t|] -> (h, t))
//      |> Array.toList
//
//  let processPath (rx:Regex) (s:string) =
//    rx.Match(s).Groups.Cast<Group>()
//    |> Seq.skip(1) // skipping the overall group
//    |> Seq.map (fun g -> g.Value)
//    |> Seq.toList


//let dispatch (ds:DispatchEntryEx list) request:HttpRequest response:ResponseData =
//    let (!!) = Query.decode
//    let absPath = !! request.Url
//    let queryString = !! request.Query
//
//    //printfn "(II) '%s' requested..." <| !! request.RawUrl
//
//    let (rx, handler) =
//        ds |> Seq.find (fun (r, _) -> r.IsMatch(request.Url))
//
//    let path = Query.processPath rx request.Url
//    let query = Query.processParams queryString
//
////    let resultBytes =
////        match handler (RequestData (path, query)) with
////        | ResponseData.Json json ->
////            let callback = Query.get "callback" "callback" query
////            json |> Encoding.UTF8.GetBytes
//
//    response.Json <- "json example"
//   // response.ContentLength64 <- int64 resultBytes.Length
//  //  response.OutputStream.Write(resultBytes, 0, resultBytes.Length)
