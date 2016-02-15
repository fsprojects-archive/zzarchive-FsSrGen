module FsSrGenTest

open System
open System.Reflection
open System.Resources
open Xunit

[<Fact>]
let ``simple with string argument``() =
    let msg = FSComp.SR.sayHello "World"
    Assert.Equal("Hello 'World'!", msg)

[<Fact>]
let ``tupled with string argument``() =
    let errNum,errMsg = FSComp.SR.invalidName "Kilo"
    Assert.Equal(203, errNum)
    Assert.Equal("Invalid name 'Kilo'", errMsg)
