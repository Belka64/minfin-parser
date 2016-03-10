open Minfin.MinfinRawData

[<EntryPoint>]
let main argv = 
    
    let asd = GetRawData
    printfn "%d" asd.Length
    0 // return an integer exit code
