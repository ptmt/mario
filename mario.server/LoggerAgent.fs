﻿module Mario.LoggerAgent

open System
open System.Threading

// Synchronous calls may either return a value or propagate an exception.
type SyncReply =
  | Value of obj
  | Exception of Exception

// Two types of messages are used by the mailbox processor:
//  Asynchronous messages take a unary procedure and an argument.
//  Synchronous messages take a unary function, an argument and a reply channel for the result.
type Message =
  | AsyncCall of (obj->unit) * obj
  | SyncCall of (obj->obj) * obj * AsyncReplyChannel<SyncReply>

/// Wraps a mailbox processor for easier implementation of active objects.
type Agent() =
   let agent = MailboxProcessor.Start( fun inbox ->
      async {
         while true do
            let! msg = inbox.Receive()
            match msg with
            | AsyncCall(f, args) ->
                try
                    f args
                with
                    | ex -> printfn "Warning: exception in asynchronous call (%A)" ex
            | SyncCall(f, args, replyChannel) ->
                try
                    f args |> Value |> replyChannel.Reply
                with
                    | ex -> ex |> Exception |> replyChannel.Reply     
      })
   
   member x.Async (f:'T->unit) (args:'T) =
      let f' (o:obj) = f (o :?> 'T)
      agent.Post( AsyncCall(f', args) )

   member x.Sync (f:'T->'U) (args:'T) : 'U =
      let f' (o:obj) = f (o :?> 'T) :> obj
      let reply = agent.PostAndReply( fun replyChannel -> SyncCall (f', args, replyChannel) )
      match reply with
      | Exception ex -> raise ex
      | Value v -> v :?> 'U
  

// Example: a simple Logger (supports two log levels, writes to stdout)

type LogLevel = Debug=1 | Error=2

type Logger(?logLevel) =
   let mutable logLevel = defaultArg logLevel LogLevel.Error
   let mutable lastMessage = None

   // implement functionality as private let-bound functions
   //  - use tuples if more than one argument is needed
   //  - only synchronously used functions should throw exceptions

   let log(level, line:string) =
      if level >= logLevel then
         lastMessage <- Some line
         let th = Thread.CurrentThread
         let loglevelstr = level.ToString()
        // Printf.kprintf (printfn "[%s][%O]\t|tThreadId=%d,\tIsTP=%b,\tIsBg=%b\t|\t%s" loglevelstr DateTime.Now th.ManagedThreadId th.IsThreadPoolThread th.IsBackground) line
         printfn "[%s][%O]\t|tThreadId=%d,\tIsTP=%b,\tIsBg=%b\t|\t%s" loglevelstr DateTime.Now th.ManagedThreadId th.IsThreadPoolThread th.IsBackground line
         //let datePrintfn (sprintf "%A: %s" DateTime.Now) format
         

   let getLastMessage() =
      match lastMessage with
      | None -> failwith "no last message"
      | Some m -> m

   // expose asynchronous and synchronous methods using an agent

   let agent = new Agent()

   member x.LogError line = agent.Async log (LogLevel.Error, line)
   member x.LogDebug line = agent.Async log (LogLevel.Debug, line)

   member x.LastMessage = agent.Sync getLastMessage ()

