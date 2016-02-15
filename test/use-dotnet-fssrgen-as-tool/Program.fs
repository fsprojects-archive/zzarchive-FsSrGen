module FsSrGenTest

open System
open System.Reflection
open System.Resources

[<EntryPoint>]
let main argv = 

    let msg = FSComp.SR.sayHello "World"
    printfn "%s" msg

    let errNum,errMsg = FSComp.SR.invalidName "Kilo"
    printfn "Err %i: %s" errNum errMsg

    0
