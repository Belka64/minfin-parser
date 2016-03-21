namespace Minfin
open FSharp.Data
open System
open Akka.FSharp
open Akka
open Akka.Actor
open FSharp.Data

type Record = 
    {
        City : string;
        Action : string;
        Currency : string;
        Url : string;
        BidId : int;
        DealHtml : HtmlNode;
    }


module Actors =
    open Minfin.Dal
    open System.Threading
    open System.Net
    open FSharp.Data.JsonExtensions
    let StartSystem =
        let cc = CookieContainer()   
        let q =Http.RequestString("http://minfin.com.ua/currency/auction/usd/sell/kharkov/",
                headers = ["Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"; 
                "Accept-Encoding", "gzip, deflate, sdch"; 
                "Accept-Language","ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4";
                "User-Agent","Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                "DNT", "1";
                "Upgrade-Insecure-Requests", "1";], 
                 cookieContainer = cc)        
        
        let system = ActorSystem.Create("minfin")
        let pageProcessor = 
            spawn system "pageprocessor" (actorOf2 (fun mailbox m -> (
                                                                      let nodeProcessor = select "akka://minfin/user/nodeprocessor" system
                                                                      let GetBidNum (node:HtmlNode)  =
                                                                          node.Descendants (fun c -> c.HasClass "js-showPhone au-dealer-phone-xxx") |> Seq.find (fun x-> true) |> fun x-> x.AttributeValue "data-bid-id" |> int
                                                                      match m with
                                                                      | (url:string, city:string, action:string, currency:string)-> 
                                                                          printfn "Downloading %s; TI is %d" city Thread.CurrentThread.ManagedThreadId
                                                                          let htmlData = HtmlDocument.Load url 
                                                                                      |> (fun x -> x.Descendants ["div"]) 
                                                                                      |> Seq.filter (fun x-> x.HasClass "au-deal-row js-deal-row-default") 
                                                                                      |> Seq.map (fun n -> {City = city; Action = action; Currency = currency;  DealHtml = n; Url = url; BidId = (GetBidNum n)})
                                                                          for d in htmlData do
                                                                              nodeProcessor <! d
                                                                      | _ ->  failwith "unknown message")))
        let nodeprocessor = 
            spawn system "nodeprocessor" (actorOf2 (fun mailbox m -> (
                                                                      let GetText (node:HtmlNode) nodeName = 
                                                                              node.Descendants (fun c -> c.HasClass nodeName) |> Seq.find (fun x-> true) |> fun x-> x.InnerText()
                                                                      let MakeSum (data:string) = 
                                                                          let processStr (d:string) =  
                                                                              d.Substring(0, d.Length - 1)
                                                                          int (processStr (data.Replace(" ", "")))
                                                                      let GetBidNum (node:HtmlNode)  =
                                                                          node.Descendants (fun c -> c.HasClass "js-showPhone au-dealer-phone-xxx") |> Seq.find (fun x-> true) |> fun x-> x.AttributeValue "data-bid-id" |> int
                                                                      let GetPhoneNum node url = 
                                                                          let bidId = GetBidNum node
                                                                          let outsidePieceOfPhone =
                                                                              GetText node "au-dealer-phone"
                                                                          let insidePieceOfPhone = 
                                                                              Http.RequestString("http://minfin.com.ua/modules/connector/connector.php"
                                                                              ,query=["action", "auction-get-contacts"; "bid", string (bidId + 1); "r", "true"]
                                                                              ,body = FormValues ["bid", string bidId; "action", "auction-get-contacts"; "r", "true"]
                                                                              ,headers = ["Accept","*/*";
                                                                              "Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4";
                                                                              "DNT", "1";
                                                                              "Origin", "http://minfin.com.ua";
                                                                              "Referer", url;
                                                                              "User-Agent","Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                                                                              "X-Requested-With","XMLHttpRequest"],cookieContainer = cc) |> JsonValue.Parse |> (fun x-> x?data.AsString())
                                                                          outsidePieceOfPhone.Replace("xxx-x", insidePieceOfPhone)
                                                                      let rep = DapperRepo()
                                                                      match rep.IsExisted(m.BidId) with
                                                                      | false ->
                                                                          printfn "Node processing: %d" m.BidId
                                                                          GetPhoneNum m.DealHtml m.Url |> ignore
                                                                          let r = m.DealHtml
                                                                          rep.AddRecord((System.DateTime.Parse(GetText r "au-deal-time")),(GetText r "au-deal-currency"), MakeSum(GetText r "au-deal-sum"), (GetPhoneNum m.DealHtml m.Url), m.City, m.Action, m.Currency, m.BidId, System.Decimal.Parse (GetText r "au-deal-currency"))
                                                                      | true -> printfn "Node skipping: %d" m.BidId
                                                                      | _ ->  failwith "unknown message")))
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
        for i in (GetAuctionUrls currencies actions cities) do
            pageProcessor <! i
        
        0

