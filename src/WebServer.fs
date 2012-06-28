module Mario.WebServer

open System.Net.Sockets
open System.Net
open Mario.Socket
open Mario.LoggerAgent
open Mario.HttpContext
open FParsec

let LOG_PATH = "c:\\data\\mario.log"
let MAX_ACCEPT_OPS = 8;
let NUMBER_OF_SEND_OPS = 8;

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
        let writeResponse (s:string) (add: string) =            
           
           let response = "HTTP/1.1 200 OK\r\nServer: MarIO/0.1.0\r\nContent-Type: application/x-javascript" + add + "\r\nConnection: Keep-Alive\r\nContent-Length: " + (System.Text.Encoding.UTF8.GetBytes s).GetLength(0).ToString() + "\r\n\r\n" + s;
           System.Text.Encoding.UTF8.GetBytes response 

        let getResponse (s:string) =
            let req = ParseRequest s            
            match req with
                | None -> writeResponse "parsing request error" ""
                | _ -> 
                    let sessionID, add = 
                        let cookies = req.Value.Headers |> List.filter (fun x-> x.Contains("Cookie: SID=")) 
                        if cookies.Length > 0 then 
                            let c = cookies |> List.head 
                            (c.Replace("Cookie: SID=", ""), "")
                        else
                            let newses = Mario.Session.genSessionId
                            (newses, "\r\nSet-Cookie:SID=" + newses)
                    let res = handler { Method = req.Value.Method; Uri = req.Value.Uri; Headers = req.Value.Headers; Body = req.Value.Body; SessionId = sessionID}
                    writeResponse res.Json add

                        

        use log = new LogAgent(LOG_PATH)
        log.info (sprintf "starting mario.server at localhost:%i" port)

        // create two socket pools
        let poolOfRecSendEventArgs = new Mario.SocketPool.SocketAsyncEventArgsPool(NUMBER_OF_SEND_OPS);

        let poolOfAcceptEventArgs = new Mario.SocketPool.SocketAsyncEventArgsPool(MAX_ACCEPT_OPS);

        async {
            try
                while (true) do
                    let! clientSocket = Mario.Socket.Accept serverSocket poolOfAcceptEventArgs
                    log.debug "client connected"
                    let buf = new System.ArraySegment<byte>(buffer, 0, 10000)
                    let! res = Mario.Socket.Receive clientSocket buf poolOfRecSendEventArgs             
                    let strings = System.Text.Encoding.UTF8.GetString (buf.Array, 0, res)                                        
                    log.debug (sprintf "recieved %A bytes" res)  
            
                    let bufR = getResponse strings
               
                    log.debug "response parsed successfully, prepare to socket send"
                    let send = new System.ArraySegment<byte>(bufR, 0, bufR.GetLength(0))
                    do! Mario.Socket.Send clientSocket send poolOfRecSendEventArgs     
                    clientSocket.Close()
                    log.debug "clientSocket.Close()" 
                    //
            with
            | ex ->
                log.error (sprintf "exception %A" ex.Message)
        } |> Async.Start // run in the thread pool    
        
        printfn "starting mario.server at localhost:%i .." port
  
        System.Console.ReadKey(true) |> ignore
        log.flush()
  
 
