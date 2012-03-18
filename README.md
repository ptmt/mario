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
           | _ -> { Json = "Others" }

    Mario.Start(myHandler)

And type in browser `http://localhost:8787` for your first web response.
