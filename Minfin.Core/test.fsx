#r @"D:\fsharp\FSharp.Data.dll"
open FSharp.Data
open System.Net
open System
open FSharp.Data.JsonExtensions

let currencies = ["usd";"eur";"rub"]
let actions = ["buy";"sell"]
let cities = ["kiev";"vinnitsa";"dnepropetrovsk";"donetsk";"zhitomir";
                "zaporozhye";"ivano-frankovsk";"kievobl";"kirovograd";"lugansk";"lutsk";
                "lvov";"nikolaev";"odessa";"poltava";"rovno";"sumy";"ternopol";
                "uzhgorod";"kharkov";"kherson";"khmelnitskiy";"cherkassy";"chernigov";"chernovtsy"]

let BuildUrl currency action city =
    sprintf "http://minfin.com.ua/currency/auction/%s/%s/%s/" currency action city 

let GetAuctionUrls currencies actions cities = 
    List.map (fun cur-> BuildUrl cur, cur) currencies 
    |> (fun funcs -> List.collect (fun act -> List.map (fun (f, cur) -> f act, act, cur) funcs) actions ) 
    |> (fun funcs -> List.collect (fun city -> List.map (fun (f, act, cur) -> f city, city, act, cur) funcs) cities ) 



let build =
    List.reduce (fun acc el -> List.collect (fun c -> c) 
                                            (List.map (fun t-> List.map (fun x-> sprintf "%s/%s" x t ) acc) el) )
                [["http://minfin.com.ua/currency/auction/"]; currencies; actions; cities]


