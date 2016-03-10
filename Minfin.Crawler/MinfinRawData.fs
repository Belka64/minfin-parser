namespace Minfin
open FSharp.Data
open System
type Record = 
    {
        City : string;
        Action : string;
        Currency : string;
        DealHtml : HtmlNode option
        DealTime : DateTime option;
        DealRate : decimal option;
        DealSum : int option;
    }
module MinfinRawData = 
    let GetRawData =    
        let currencies = ["usd"]//;"eur";"rub"]
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
                                                        
        let GetData urls = 
            let GetBids uri = 
                printf "Load html from %s" uri
                let results = HtmlDocument.Load(uri)
                results.Descendants ["div"]
                |> Seq.filter (fun x-> x.HasClass "au-deal-row js-deal-row-default")
                |> List.ofSeq 
            List.collect (fun (url, city, action, currency)-> List.map (fun x-> {City = city; Action = action; Currency = currency; DealTime = None; DealRate = None; DealSum = None; DealHtml = Some(x)}) (GetBids url)) urls
        GetAuctionUrls currencies actions cities |> GetData

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code