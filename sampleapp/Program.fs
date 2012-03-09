open Mario.Socket
open Mario.LoggerAgent
open Mario.HttpContext
open Mario.WebServer


let myhandler (req:HttpRequest) : HttpResponse =
    match req.Method with
        | HttpMethod.GET -> { Json = "just get request" }
        | HttpMethod.POST -> { Json = req.Body }
        | _ -> { Json = "not some" }

Mario.Start(myhandler)


