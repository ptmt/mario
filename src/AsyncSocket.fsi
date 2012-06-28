module Mario.Socket 
open System.Net.Sockets
open Mario.SocketPool

/// Thrown when sockets encounter errors.
exception SocketIssue of SocketError

/// Performs AcceptAsync.
val Accept : Socket -> SocketAsyncEventArgsPool -> Async<Socket> 

/// Performs ReceiveAsync.
val Receive : Socket -> System.ArraySegment<byte>  -> SocketAsyncEventArgsPool-> Async<int>

/// Performs SendAsync.
val Send : Socket -> System.ArraySegment<byte>  -> SocketAsyncEventArgsPool-> Async<unit>
