



let t = System.DateTime.Parse "08:55"

let MakeSum (data:string) = 
            let processStr (d:string) =  
                d.Substring(0, d.Length - 1)
            int (processStr (data.Replace(" ", "")))

MakeSum "28 000 $"


let make (d:string) =
    let p d =  
        System.Decimal.Parse d
    p d

make "27,8"