open Minfin.Dal
open FSharp.Data
open System.Net
open System
open FSharp.Data.JsonExtensions

[<EntryPoint>]
let main argv = 
    let rep = new Repository()
    let currencies = ["usd"]//;"eur";"rub"]
    let actions = ["buy";"sell"]
    let cities = ["kiev";"vinnitsa";"dnepropetrovsk";"donetsk";"zhitomir";
                    "zaporozhye";"ivano-frankovsk";"kievobl";"kirovograd";"lugansk";"lutsk";
                    "lvov";"nikolaev";"odessa";"poltava";"rovno";"sumy";"ternopol";
                    "uzhgorod";"kharkov";"kherson";"khmelnitskiy";"cherkassy";"chernigov";"chernovtsy"]
    
    let BuildUrl currency action city =
        sprintf "http://minfin.com.ua/currency/auction/%s/%s/%s/" currency action city 
    
    let url = BuildUrl  "usd"  "sell" "kharkov" 
    let cc = CookieContainer()
    let req = Http.RequestString(url,
                        headers = ["Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"; 
                        "Accept-Encoding", "gzip, deflate, sdch"; 
                        "Accept-Language","ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4";
                        "User-Agent","Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                        "DNT", "1";
                        "Upgrade-Insecure-Requests", "1";], 
                         cookieContainer = cc) |> ignore
                        
    let GetAuctionUrls currencies actions cities = 
        List.map (fun cur-> BuildUrl cur, cur) currencies 
        |> (fun funcs -> List.collect (fun act -> List.map (fun (f, cur) -> f act, act, cur) funcs) actions ) 
        |> (fun funcs -> List.collect (fun city -> List.map (fun (f, act, cur) -> f city, city, act, cur) funcs) cities ) 
                                                        
    let GetData urls = 
        let GetBids (uri:string) = 
            let results = HtmlDocument.Load(uri)
            results.Descendants ["div"]
            |> Seq.filter (fun x-> x.HasClass "au-deal-row js-deal-row-default")
            |> List.ofSeq 
        List.map (fun (url, city, action, currency)-> GetBids url, city, action, currency) urls

    
    let ProcessData data =
        let GetText (node:HtmlNode) nodeName = 
                node.Descendants (fun c -> c.HasClass nodeName) |> Seq.find (fun x-> true) |> fun x-> x.InnerText()
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
        let MakeSum (data:string) = 
            let processStr (d:string) =  
                d.Substring(0, d.Length - 1)
            int (processStr (data.Replace(" ", "")))
        let GetDataFromHtmlRow (row:HtmlNode list) = 
            List.map (fun (x:HtmlNode) -> GetText x "au-deal-time", GetText x "au-deal-currency", GetText x "au-deal-sum", x, GetBidNum x) row |>
            List.filter (fun (_,_,_,_,q) -> rep.Existed q)
        data |> List.collect(fun ((dataSeq:HtmlNode list), city, action, currency) -> (GetDataFromHtmlRow dataSeq) |> List.map (fun (dealtime, curRank, sum, node, bidNum) -> dealtime, curRank, MakeSum sum, GetPhoneNum node, bidNum, city, action, currency)) 
    
    let Save x =
        let savetodb x = 
            let makeTime t =
                System.DateTime.Parse t
            match x with
            | (dealtime, curRank, sum, phone, bidNum, city, action, currency) -> rep.AddRecord(makeTime dealtime, curRank, sum, phone, city, action, currency, bidNum)
        List.iter (fun q-> savetodb q) x

    let data = GetAuctionUrls currencies actions cities |> GetData |> ProcessData |> Save
    0