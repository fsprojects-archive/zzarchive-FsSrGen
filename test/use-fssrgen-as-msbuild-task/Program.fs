module FsSrGenTest

open System
open System.Reflection
open System.Resources

open NUnit.Framework

[<Test>]
let ``simple with string argument``() =
    let msg = FSComp.SR.sayHello "World"
    Assert.AreEqual("Hello 'World'!", msg)

[<Test>]
let ``tupled with string argument``() =
    let errNum,errMsg = FSComp.SR.invalidName "Kilo"
    Assert.AreEqual(203, errNum)
    Assert.AreEqual("Invalid name 'Kilo'", errMsg)

open System
open System.Reflection

type Program = class end

[<EntryPoint>]
let main argv = 

#if NETSTANDARD1_5
    let run = typeof<Program>.GetTypeInfo().Assembly |> NUnitLite.AutoRun
    run.Execute(argv, (new NUnit.Common.ExtendedTextWrapper(Console.Out)), Console.In)
#else
    let run = NUnitLite.AutoRun()
    run.Execute(argv)
#endif
