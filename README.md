#mar.io

High perfomance, easy, small, http application server for JSON-processing. 

Preview Version

##Getting Started

Create F# console application:
    
    open Mario.HttpContext
    open Mario.WebServer


    let myHandler (req:HttpRequest) : HttpResponse =
        match req.Method with
           | HttpMethod.GET -> { Json = "Yet Another GET Request" }
           | HttpMethod.POST -> { Json = req.Body }
           | _ -> { Json = "Others Requests" }

    Mario.Start(myHandler, 8787)

And type in browser `http://localhost:8787` to see your first web response.
