
#mar.io

Легкий и крайне урезанный веб-сервер, использующий IO Completion Port и Async-возможности f#. 

##Getting Started
    
    open Mario.HttpContext
    open Mario.WebServer


    let myHandler (req:HttpRequest) : HttpResponse =
        match req.Method with
           | HttpMethod.GET -> { Json = "Yet Another GET Request" }
           | HttpMethod.POST -> { Json = req.Body }
           | _ -> { Json = "Others" }

    Mario.Start(myHandler)

And type in browser `http://localhost:8787` for your first web response.
