module Mario.Server

open System.Net.Sockets
open Mario.Socket

let PORT = 65000

let serverSocket = Socket.createListening PORT

//let income = ref null

Socket.Incoming |> Observable.add(fun s ->
  printfn "socket accepted"
  //ignore incoming sockets, except for the first one
 // if box !income = null then income := s else printfn "forget it, we have one already"
  //accept more sockets...
//  Socket.accept serverSocket
)
Socket.Connected |> Observable.add (fun s ->
  printfn "socket connected [%s:%d]" (Socket.ip s) (Socket.port s)
)
Socket.Disconnected |> Observable.add (fun (s, reason) ->
  printfn "socket purposely disconnected [%s:%d] because %s" (Socket.ip s) (Socket.port s) reason
)
Socket.SentData|> Observable.add (fun (s, n) ->
  printfn "socket [%s:%d] sent %d bytes" (Socket.ip s) (Socket.port s) n
)
Socket.ReceivedData |> Observable.add (fun (s, n) ->
  printfn "socket [%s:%d] received %d bytes" (Socket.ip s) (Socket.port s) n
)

Socket.accept serverSocket

printfn "waiting"
    
System.Console.ReadKey(true) |> ignore

//
//let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//socket |> Socket.connect "127.0.0.1" PORT
//
//let socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//socket2 |> Socket.connect "127.0.0.1" PORT
//
//let buffer = Array.create 100 0uy
//let buffer2 = Array.create 500 1uy
//
//!income |> Socket.send buffer 0 buffer.Length
////nothing should happen, the event will be triggered when all data will have been read
//socket |> Socket.receiveUntil buffer2 0 buffer2.Length
//!income |> Socket.send buffer 0 buffer.Length
//!income |> Socket.send buffer 0 buffer.Length
//!income |> Socket.send buffer 0 buffer.Length
//!income |> Socket.send buffer 0 buffer.Length //the ReceivedData is emitted here
//

