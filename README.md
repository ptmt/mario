#mar.io


High perfomance, easy, small, http application server for JSON-processing. 

This is Preview Version

For trying just create F# console application with referenced mario.server.dll:
    
    open Mario.HttpContext
    open Mario.WebServer


    let myHandler (req:HttpRequest) : HttpResponse =
        match req.Method with
           | HttpMethod.GET -> { Json = "Yet Another GET Request" }
           | HttpMethod.POST -> { Json = req.Body }
           | _ -> { Json = "Others Requests" }

    Mario.Start(myHandler, 8787)

And type in browser `http://localhost:8787` to see your first web response.
