module Mario.WebServer

open System.Net.Sockets
open System.Net
open Mario.Socket
open Mario.LoggerAgent
open Mario.HttpContext
open FParsec

type Mario() =     

    static member Start(handler : HttpRequest -> HttpResponse, ?port) =

        let create() =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        let createListeningExtended ip port n =
            let s = create()
            let ip =
                match ip with
                | Some s -> IPAddress.Parse s
                | None -> IPAddress.Any
            s.Bind(new IPEndPoint(ip, port))
            s.Listen(n)
            s
        let createListening port = createListeningExtended None port (int SocketOptionName.MaxConnections)
        let port = defaultArg port 8787
        let serverSocket = createListening port
        let buffer = Array.create 10000 0uy

        ///TODO make writeResponse to global buffer POOL
        let writeResponse (s:string) =            
           let response = "HTTP/1.1 200 OK\r\nServer: MarIO/0.0.1\r\nContent-Type: application/x-javascript\r\nContent-Length: " + (System.Text.Encoding.UTF8.GetBytes s).GetLength(0).ToString() + "\r\n\r\n" + s;
           System.Text.Encoding.UTF8.GetBytes response 

        let getResponse (s:string) =
            let req = ParseRequest s
            match req with
                | None -> writeResponse "parsing request error"
                | _ -> 
                    let res = handler req.Value
                    writeResponse res.Json

                        

        let logger = new Logger(LogLevel.Debug)
        logger.LogDebug "starting server"    
        async {
            try
                while (true) do
                    let! clientSocket = Mario.Socket.Accept serverSocket    
                    logger.LogDebug "client connected"
                    let buf = new System.ArraySegment<byte>(buffer, 0, 10000)
                    let! res = Mario.Socket.Receive clientSocket buf             
                    let strings = System.Text.Encoding.UTF8.GetString (buf.Array, 0, res)                    
                    logger.LogDebug (sprintf "recieved %A bytes" res)  
                    logger.LogDebug strings
                   // let handleRequest = defaultArg processContext (fun x -> new ResponseData{Json="Hello World!"})
                    
                    let bufR = getResponse strings
                 //   let stringsR = System.Text.Encoding.UTF8.GetString (bufR, 0, bufR.GetLength(0))      
                 //   logger.LogDebug (sprintf "sending %A" stringsR)
                    let send = new System.ArraySegment<byte>(bufR, 0, bufR.GetLength(0))
                    do! Mario.Socket.Send clientSocket send     
                    logger.LogDebug "Socket.Send, loop end" 
            with
            | ex ->
                logger.LogError (sprintf "exception %A" ex.Message)
        } |> Async.Start // run in the thread pool    
        
        printfn "starting mario.server at localhost:%i" port
  
        System.Console.ReadKey(true) |> ignore
 
  
 
