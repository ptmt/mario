module Mario.Socket 
    open System.Net.Sockets
    open System.Net
    open Mario.SocketPool

    type A = System.Net.Sockets.SocketAsyncEventArgs
    type B = System.ArraySegment<byte>

    exception SocketIssue of SocketError with
        override this.ToString() =
            string this.Data0

    /// Wraps the Socket.xxxAsync logic into F# async logic.
    /// op A -> bool ? func operation. Accept, Recieve, Send 
    ///         http://msdn.microsoft.com/en-us/library/system.net.sockets.socket.acceptasync.aspx true if the I/O operation is pending. false if the I/O operation completed synchronously.
    /// prepare A -> unit ? function for prepare SocketAsyncEventArgs
    /// select A -> 'T ?  функция фильтрации
    /// return Async<'T>  async result 
    let inline asyncDo 
        (op: A -> bool)
        (prepare: A -> unit)
        (select: A -> 'T)
        (pool:SocketAsyncEventArgsPool) =

        Async.FromContinuations <| fun (ok, error, _) ->
            let args =
                if pool.Count > 0 
                then 
                    //printfn "pop saea"
                    pool.Pop()
                else
                    //printfn "new saea"
                    new A()                  
            
            prepare args
            let processSocket (args: A) =
                match args.SocketError with
                | System.Net.Sockets.SocketError.Success ->                              
                    let result = select args
                
                   // pool.Push args
                    ok result
                | e ->
                  //  args.AcceptSocket.Close()
                   // pool.Push args
                    error (SocketIssue e)
                 
            args.add_Completed(System.EventHandler<_>(fun _ -> processSocket)) // asynchronously
            if not (op args) then
                processSocket args // synchronyously

    /// Prepares the arguments by setting the buffer.
    let inline setBuffer (buf: B) (args: A) =     
        args.SetBuffer(buf.Array, buf.Offset, buf.Count)

    let Accept (socket: Socket) (pool: SocketAsyncEventArgsPool) =
        asyncDo socket.AcceptAsync ignore (fun a -> a.AcceptSocket) pool

    let Receive (socket: Socket) (buf: B) (pool: SocketAsyncEventArgsPool)=
        asyncDo socket.ReceiveAsync (setBuffer buf) (fun a -> a.BytesTransferred) pool

    let Send (socket: Socket) (buf: B) (pool: SocketAsyncEventArgsPool)=
        asyncDo socket.SendAsync (setBuffer buf) ignore pool
