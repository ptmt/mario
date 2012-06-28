module Mario.LoggerAgent

open System
open System.Threading



// Synchronous calls may either return a value or propagate an exception.
//type SyncReply =
//  | Value of obj
//  | Exception of Exception

// Two types of messages are used by the mailbox processor:
//  Asynchronous messages take a unary procedure and an argument.
//  Synchronous messages take a unary function, an argument and a reply channel for the result.
//type Message =
//  | AsyncCall of (obj->unit) * obj
//  | SyncCall of (obj->obj) * obj * AsyncReplyChannel<SyncReply>


type private Message =
        | Debug   of string
        | Info    of string
        | Warn    of string
        | Error   of string
        | Fatal   of string
    with
        static member toString logMessage =
            match logMessage with
            | Debug   m -> m |> sprintf "DEBUG:%s"
            | Info    m -> m |> sprintf "INFO:%s"
            | Warn    m -> m |> sprintf "WARN:%s"
            | Error   m -> m |> sprintf "ERROR:%s"
            | Fatal   m -> m |> sprintf "FATAL:%s"
    
        override this.ToString() =
            Message.toString this

type private LogCommand =
    | Log of Message
    | Flush
    | Close of AsyncReplyChannel<unit>


    type LogAgent(logFile:string) as this  =
        let writer = lazy(System.IO.File.AppendText logFile)

        let agent = MailboxProcessor.Start (fun agent ->
            // Do the loop until the Stop command is received
            // Keep the number of lines written to the log
            let rec loop(count) = async {
                let! command = agent.Receive()
                match command with
                | Log message -> 
                    let count = count + 1
                    let th = Thread.CurrentThread
                    //let message = Message.toString message
                    let message = sprintf "[%O, %i]\t|tThreadId=%d,\tIsTP=%b,\tIsBg=%b\t|\t%s" DateTime.Now DateTime.Now.Millisecond th.ManagedThreadId th.IsThreadPoolThread th.IsBackground (Message.toString message)
                    writer.Value.WriteLine (message)
                    return! loop(count)
                | Flush ->
                    if writer.IsValueCreated then
                        writer.Value.Flush()
                    return! loop(count)
                | Close reply ->
                    let message = sprintf "%d messages written into log" count
                    Console.WriteLine message
                    this.doClose()
                    reply.Reply(ignore())
                    return ignore()
            }

            loop(0))

        interface IDisposable with
            member this.Dispose() = this.doClose()
    
        member private this.doClose() = 
            let message = sprintf "Discarding %d messages in the queue" (agent.CurrentQueueLength)
            Console.WriteLine(message)

            let d = agent :> IDisposable
            d.Dispose()

            if writer.IsValueCreated then
                writer.Value.Dispose()
    
        member private this.log objToMessage obj = 
            obj |> objToMessage |> LogCommand.Log |> agent.Post 


        member this.fatal = this.log Fatal;
        member this.error = this.log Error
        member this.warn  = this.log Warn
        member this.info  = this.log Info
        member this.debug = this.log Debug

        member this.queueLength = agent.CurrentQueueLength
        member this.flush() = LogCommand.Flush |> agent.Post
        member this.close() = LogCommand.Close |> agent.PostAndReply

//
///// Wraps a mailbox processor for easier implementation of active objects.
//type Agent() =
//   let agent = MailboxProcessor.Start( fun inbox ->
//      async {
//         while true do
//            let! msg = inbox.Receive()
//            match msg with
//            | AsyncCall(f, args) ->
//                try
//                    f args
//                with
//                    | ex -> printfn "Warning: exception in asynchronous call (%A)" ex
//            | SyncCall(f, args, replyChannel) ->
//                try
//                    f args |> Value |> replyChannel.Reply
//                with
//                    | ex -> ex |> Exception |> replyChannel.Reply     
//      })
//   
//   member x.Async (f:'T->unit) (args:'T) =
//      let f' (o:obj) = f (o :?> 'T)
//      agent.Post( AsyncCall(f', args) )
//
//   member x.Sync (f:'T->'U) (args:'T) : 'U =
//      let f' (o:obj) = f (o :?> 'T) :> obj
//      let reply = agent.PostAndReply( fun replyChannel -> SyncCall (f', args, replyChannel) )
//      match reply with
//      | Exception ex -> raise ex
//      | Value v -> v :?> 'U
//  
//
//// Example: a simple Logger (supports two log levels, writes to stdout)
//
//type LogLevel = Debug=1 | Error=2
//
//type Logger(?logLevel) =
//   let mutable logLevel = defaultArg logLevel LogLevel.Error
//   let mutable lastMessage = None
//
//   // implement functionality as private let-bound functions
//   //  - use tuples if more than one argument is needed
//   //  - only synchronously used functions should throw exceptions
//
//   let log(level, line:string) =
//      if level >= logLevel then
//         lastMessage <- Some line
//         let th = Thread.CurrentThread
//         let loglevelstr = level.ToString()
//         loglevelstr |> ignore
//        // Printf.kprintf (printfn "[%s][%O]\t|tThreadId=%d,\tIsTP=%b,\tIsBg=%b\t|\t%s" loglevelstr DateTime.Now th.ManagedThreadId th.IsThreadPoolThread th.IsBackground) line
//        // use file = System.IO.File.AppendText(LOG_PATH)
//        // let logm = sprintf "[%s][%O, %i]\t|tThreadId=%d,\tIsTP=%b,\tIsBg=%b\t|\t%s" loglevelstr DateTime.Now DateTime.Now.Millisecond th.ManagedThreadId th.IsThreadPoolThread th.IsBackground line
//        // logm |> ignore
//        // file.WriteLine(logm)
//         //let datePrintfn (sprintf "%A: %s" DateTime.Now) format
//         
//
//   let getLastMessage() =
//      match lastMessage with
//      | None -> failwith "no last message"
//      | Some m -> m
//
//   // expose asynchronous and synchronous methods using an agent
//
//   let agent = new Agent()
//
//   member x.LogError line = agent.Async log (LogLevel.Error, line)
//   member x.LogDebug line = agent.Async log (LogLevel.Debug, line)
//
//   member x.LastMessage = agent.Sync getLastMessage ()

