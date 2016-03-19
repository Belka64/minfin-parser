namespace Minfin
open FSharp.Data
open System
open Akka.FSharp
open Akka
open Akka.Actor
open FSharp.Data
open System.Threading
open Akka.Configuration
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


module Actors =
    open Minfin.Dal
    open System.Threading
    let StartSystem =
                                                                                        
        
        let system = ActorSystem.Create("minfin")
        let pageProcessor = spawn system "pageprocessor" (actorOf2 (fun mailbox m -> (
                                                                                        let nodeProcessor = select "akka://minfin/user/nodeprocessor" system
                                                                                        
                                                                                        match m with
                                                                                        | (url:string, city:string, action:string, currency:string)-> 
                                                                                            printfn "Downloading %s; TI is %d" city Thread.CurrentThread.ManagedThreadId
                                                                                            let htmlData = HtmlDocument.Load url 
                                                                                                        |> (fun x -> x.Descendants ["div"]) 
                                                                                                        |> Seq.filter (fun x-> x.HasClass "au-deal-row js-deal-row-default") 
                                                                                                        |> Seq.map (fun n -> {City = city; Action = action; Currency = currency;  DealHtml = n})
                                                                                            for d in htmlData do
                                                                                                nodeProcessor <! d
                                                                                        | _ ->  failwith "unknown message")))
        let nodeprocessor = spawn system "nodeprocessor" (actorOf2 (fun mailbox m -> (
                                                                                        let GetText (node:HtmlNode) nodeName = 
                                                                                                node.Descendants (fun c -> c.HasClass nodeName) |> Seq.find (fun x-> true) |> fun x-> x.InnerText()
                                                                                        match m with
                                                                                        | r ->
                                                                                            let repo = new Repository()
                                                                                            printfn "nodeprocessorthreadID is  %d %s" Thread.CurrentThread.ManagedThreadId r.City
                                                                                            //repo.AddRecord((System.DateTime.Parse(GetText r "au-deal-time")),)
                                                                                            
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
            pageProcessor.Tell i
        printfn "Actor system was ended"
        0

