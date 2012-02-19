module Mario.Socket 
open System.Net.Sockets
  type t = Socket

  type Error =
    | Connect
    | Accept
    | Send
    | Receive

  //
  // Events
  //
  val Connected : IEvent<t>

  //Generated when the disconnection is done purposely by the programmer.
  val Disconnected : IEvent<t * string>

  //Generated when an incoming connection is detected.
  val Incoming : IEvent<t>

  val SentData : IEvent<t * int>

  val ReceivedData : IEvent<t * int>

  //For connection and incoming connection errors, the SocketError is
  //set to SocketError.SocketError, which is the default undefined error.
  val Failure : IEvent<t * Error * SocketError * System.Exception>

  //
  // Constructor shorthand
  //
  val create : unit -> t

  val createListeningExtended : ip:option<string> -> port:int -> nToListen:int -> t

  val createListening : port:int -> t

  //
  // "Properties"
  //
  val ip : socket:t -> string

  val port : socket:t -> int

  val isAlive : socket:t -> bool

  val isConnected : socket:t -> bool

  //
  // Operations running asynchronously (and trigger events upon completion)
  //
  val connect : ip:string -> port:int -> Socket -> unit

  val disconnect : socket:t -> reason:string -> keepForLater:bool -> unit

  val send : data:byte[] -> offset:int -> count:int -> socket:t -> unit

  val receive : buffer:byte[] -> offset:int -> count:int -> socket:t -> unit

  //If we want to receive more byes than the buffer size, or if the data
  //is sent progressively, we shall use this function which waits until
  //all the data is received, contrary to the [receive] function which is
  //a one-shot attempt.
  val receiveUntil : buffer:byte[] -> offset:int -> count:int -> socket:t -> unit

  val accept : socket:t -> unit
