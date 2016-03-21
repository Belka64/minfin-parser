open Minfin.Actors
open System.Threading
open System
[<EntryPoint>]
let main argv = 
    StartSystem |> ignore
    Thread.Sleep 320000
    0
    
