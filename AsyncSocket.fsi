module Mario.Socket 
open System.Net.Sockets


/// Thrown when sockets encounter errors.
exception SocketIssue of SocketError

/// Performs AcceptAsync.
val Accept : Socket -> Async<Socket>

/// Performs ReceiveAsync.
val Receive : Socket -> System.ArraySegment<byte> -> Async<int>

/// Performs SendAsync.
val Send : Socket -> System.ArraySegment<byte> -> Async<unit>
