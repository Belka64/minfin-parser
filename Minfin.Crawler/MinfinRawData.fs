namespace Minfin
open FSharp.Data
type RawRecord = 
    {
        City : string;
        Action : string;
        Currency : string;
        Bids : HtmlNode list
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
            List.map (fun (url, city, action, currency)-> {City = city; Action = action; Currency = currency; Bids = (GetBids url)}) urls
        GetAuctionUrls currencies actions cities |> GetData
