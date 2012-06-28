module Mario.HttpUtility


let decode uri = 
    System.Uri.UnescapeDataString(uri)

