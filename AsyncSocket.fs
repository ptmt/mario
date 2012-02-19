module Mario.Socket 
    open System.Net.Sockets
    open System.Net
    type t = Socket  

    type Error =
    | Connect
    | Accept
    | Send
    | Receive

  //
  //  Events
  //
    let connected = new Event<t>()
    let Connected = connected.Publish

    let disconnected = new Event<t * string>()
    let Disconnected = disconnected.Publish

    let incoming = new Event<t>()
    let Incoming = incoming.Publish

    let sentData = new Event<t * int>()
    let SentData = sentData.Publish

    let receivedData = new Event<t * int>()
    let ReceivedData = receivedData.Publish

    let failure = new Event<t * Error * SocketError * System.Exception>()
    let Failure = failure.Publish 

    //
    // Constructor shorthand
    //

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

    let createListening port = createListeningExtended None port 5

    //
    // "Properties"
    //

    let ip (socket:t) =
        let ipEndPoint = socket.RemoteEndPoint :?> IPEndPoint
        ipEndPoint.Address.ToString()

    let port (socket:t) =
        let ipEndPoint = socket.RemoteEndPoint :?> IPEndPoint
        ipEndPoint.Port

    let isAlive (socket:t) = box socket <> null

    let isConnected socket =
        if isAlive socket then
            try
            not <| (socket.Poll(1, SelectMode.SelectRead) && socket.Available = 0)
            with
            | :? SocketException -> false
            | e -> failwith e.Message
        else false         

    //
    // Connection
    //
    let asyncConnect (ip:string) port (socket:Socket) =
        Async.FromBeginEnd((fun (cb,o) -> socket.BeginConnect(ip,port,cb,o)), socket.EndConnect)

    let onConnectCompleted socket () =
        connected.Trigger socket

    let onConnectFailed socket (exn:System.Exception) =
        failure.Trigger(socket, Error.Connect, SocketError.SocketError, exn)

    let connect ip port socket =
        Async.StartWithContinuations(
            asyncConnect ip port socket,
            onConnectCompleted socket,
            onConnectFailed socket,
            ignore
        )

    //
    // Disconnection
    //

    let disconnect (socket:t) (reason:string) keepForLater =
        if isConnected socket then
            try
            socket.Shutdown(SocketShutdown.Both)
            socket.Disconnect(true) //doesn't work on all platforms
            with _ -> ()
            if not keepForLater then socket.Close()
            disconnected.Trigger(socket, reason)      

    //
    // Sending data
    //

    let asyncSend data offset count err (socket:Socket) =
        Async.FromBeginEnd(
            (fun (cb,o) -> socket.BeginSend(data, offset, count, SocketFlags.None,cb,o)),
            (fun iar -> socket.EndSend(iar, err))
        )

    let onSendCompleted socket n =
        sentData.Trigger(socket, n)

    let onSendFailed socket err (exn:System.Exception) =
        failure.Trigger(socket, Error.Send, err, exn)

    let send data offset count socket =
        let err = ref <| Unchecked.defaultof<SocketError>
        Async.StartWithContinuations(
            asyncSend data offset count err socket,
            onSendCompleted socket,
            onSendFailed socket !err,
            ignore
        )

    //
    // Receiving data
    //
    let asyncReceive buffer offset count err (socket:Socket) =
        Async.FromBeginEnd(
            (fun (cb,o) -> socket.BeginReceive(buffer, offset, count, SocketFlags.None,cb,o)),
            (fun iar -> socket.EndReceive(iar, err))
        )

    let onReceiveCompleted socket n =
        receivedData.Trigger(socket, n)

    let onReceiveFailed socket err (exn:System.Exception) =
        failure.Trigger(socket, Error.Receive, err, exn)

    let receive buffer offset count socket =
        let err = ref <| Unchecked.defaultof<SocketError>
        Async.StartWithContinuations(
            asyncReceive buffer offset count err socket,
            onReceiveCompleted socket,
            onReceiveFailed socket !err,
            ignore
        )

    
    let rec onReceiveUntilCompleted buffer offset countRemaining countTotal socket n =
        let nToReceive = countRemaining - n
        if nToReceive > 0 then
            _receiveUntil buffer (offset+n) nToReceive countTotal socket
        else
            receivedData.Trigger(socket, countTotal)

    and _receiveUntil buffer offset countRemaining countTotal socket =
        let err = ref <| Unchecked.defaultof<SocketError>
        Async.StartWithContinuations(
            asyncReceive buffer offset countRemaining err socket,
            onReceiveUntilCompleted buffer offset countRemaining countTotal socket,
            onReceiveFailed socket !err,
            ignore
        )   


    let receiveUntil buffer offset count socket =
        _receiveUntil buffer offset count count socket

    //
    // Accepting an incoming socket
    //
    let asyncAccept (socket:Socket) =
        Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)

    let onAcceptCompleted socket =
        incoming.Trigger(socket)

    let onAcceptFailed socket (exn:System.Exception) =
        failure.Trigger(socket, Error.Accept, SocketError.SocketError, exn)

    let accept socket =
        Async.StartWithContinuations(
            asyncAccept socket,
            onAcceptCompleted,
            onAcceptFailed socket,
            ignore
        )
