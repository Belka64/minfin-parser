namespace Minfin
open FSharp.Data
open System
type Record = 
    {
        City : string;
        Action : string;
        Currency : string;
        DealHtml : HtmlNode;
//        DealTime : DateTime option;
//        DealRate : decimal option;
//        DealSum : int option;
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
                printfn "Load html from %s" uri
                let results = HtmlDocument.Load(uri)
                results.Descendants ["div"]
                |> Seq.filter (fun x-> x.HasClass "au-deal-row js-deal-row-default")
                |> List.ofSeq 
            List.collect (fun (url, city, action, currency)-> List.map (fun x-> {City = city; Action = action; Currency = currency;  DealHtml = x}) (GetBids url)) urls
        GetAuctionUrls currencies actions cities |> GetData

module Processer =
    open Minfin.Dal
    open System
    let Process (data:Record list) =
        let rep = new Repository()
        let GetText (node:HtmlNode) nodeName = 
            node.Descendants (fun c -> c.HasClass nodeName) |> Seq.find (fun x-> true) |> fun x-> x.InnerText()
        let MakeSum (data:string) = 
            let processStr (d:string) =  
                d.Substring(0, d.Length - 1)
            int (processStr (data.Replace(" ", "")))
        let GetBidNum (node:HtmlNode)  =
            node.Descendants (fun c -> c.HasClass "js-showPhone au-dealer-phone-xxx") |> Seq.find (fun x-> true) |> fun x-> x.AttributeValue "data-bid-id" |> int
        let GetPhoneNum node = 
            let bidId = GetBidNum node
            let bidIdForGet = string (bidId + 1)
            let outsidePieceOfPhone =
                GetText node "au-dealer-phone"
            let insidePieceOfPhone = 
                Http.RequestString("http://minfin.com.ua/modules/connector/connector.php"
                ,query=["action", "auction-get-contacts"; "bid", bidIdForGet; "r", "true"]
                ,body = FormValues ["bid", string bidId; "action", "auction-get-contacts"; "r", "true"]
                ,headers = ["Accept","*/*";
                "User-Agent","Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                "X-Requested-With","XMLHttpRequest"],cookieContainer = cc) |> JsonValue.Parse |> (fun x-> x?data.AsString())
            outsidePieceOfPhone.Replace("xxx-x", insidePieceOfPhone)
        

        List.iter (fun x -> rep.AddRecord(DateTime.Parse (GetText x.DealHtml "au-deal-time"), "",0,"","","","",1,0)) data
        0
