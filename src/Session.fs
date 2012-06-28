module Mario.Session

open ProtoBuf
open Kevo.ProtoBuf

let mainSessionId = 100000

type UserToken() = 
    let sessionId:int = System.Threading.Interlocked.Increment(ref mainSessionId)
    member x.ToString = string sessionId
    member x.GetParams() = printfn "%s" (string sessionId)

[<ProtoContract(ImplicitFields = ImplicitFields.AllPublic)>]
type SessionPair (sessionId, key, value) = class  
    member val SessionId : string = sessionId with get, set
    member val Key : string = key with get, set  
    member val Value : string = value with get, set     
    new() = SessionPair("", "", "")
    
end  

let genSessionId =
    let a = new System.Security.Cryptography.RNGCryptoServiceProvider()
    let buffer = Array.create 8 0uy
    a.GetBytes(buffer)
    System.BitConverter.ToUInt64(buffer, 0) |> string
    
let add sid key value = 
    let s = new SessionPair (sid, key, value)
    let id = Kevo.Store.lastid<SessionPair>
    Kevo.Store.append<SessionPair>(id, s, None)

let get sid key = 
    Kevo.Store.findByQuery<SessionPair> (fun x-> x.SessionId = sid && x.Key = key)