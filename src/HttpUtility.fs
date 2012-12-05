module Mario.HttpUtility

open Mario.HttpContext

let decode uri = 
    System.Uri.UnescapeDataString(uri).Replace("+"," ")

let badRequest =     
    {Json = "bad request" }
