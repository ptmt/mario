#mar.io


Mario is a high perfomance and lightweight application server for JSON-processing. It written in F# and based on IO Completion Port (also worked under Mono).

Currently under development.

For trying just create F# console application with referenced mario.server.dll:
    
    open Mario.HttpContext
    open Mario.WebServer


    let myHandler (req:HttpRequest) : HttpResponse =
        match req.Method with
           | HttpMethod.GET -> { Json = "Yet Another GET Request" }
           | HttpMethod.POST -> { Json = req.Body }
           | _ -> { Json = "Not Implemented" }

    Mario.Start(myHandler, 8787)

Compile it with FAKE or xbuild and type in browser `http://localhost:8787` to see your first HTTP-response.
