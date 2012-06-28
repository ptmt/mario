module Mario.SocketPool

open System.Net.Sockets
open System.Threading

type SocketAsyncEventArgsPool(capacity: int)  =

    //just for assigning an ID so we can watch our objects while testing.
    let mutable nextTokenId  = 0

    // Pool of reusable SocketAsyncEventArgs objects.
    // "capacity" = Maximum number of SocketAsyncEventArgs objects
    let pool = new System.Collections.Generic.Stack<SocketAsyncEventArgs>(capacity)

    
    // The number of SocketAsyncEventArgs instances in the pool.
    member this.Count = pool.Count

    member this.AssignTokenId() =     
        Interlocked.Increment(ref nextTokenId)        
    

    // Removes a SocketAsyncEventArgs instance from the pool.
    // returns SocketAsyncEventArgs removed from the pool.
    member this.Pop() =     
        lock pool (fun () -> pool.Pop())
    

    // Add a SocketAsyncEventArg instance to the pool.
    // "item" = SocketAsyncEventArgs instance to add to the pool.
    member this.Push (item:SocketAsyncEventArgs ) =     
        if item = null then
            raise (new System.ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"))
        lock pool (fun () -> pool.Push(item))
        
    

let init (pool: SocketAsyncEventArgsPool, size:int) = 
    [1..size] |> List.iter (fun (i) ->
        let acceptEventArg = new SocketAsyncEventArgs() //

       // acceptEventArg.Completed +=
       //         new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

       // AcceptOpUserToken theAcceptOpToken = new
      //  			AcceptOpUserToken(pool.AssignTokenId() + 10000);

      //  acceptEventArg.UserToken = theAcceptOpToken;        
        pool.Push(acceptEventArg))