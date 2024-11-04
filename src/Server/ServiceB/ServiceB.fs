module ServiceB

open Dapr.Actors.Client
open Dapr.Actors.Runtime
open Serilog

open Shared.Actors

type ServiceB(host: ActorHost) =
    inherit Actor(host)

    let mutable count = 0
    member this.myId = this.Id.GetId()

    interface IServiceA with
        member this.Foo() =
            task {
                let proxy = ActorProxy.Create<IServiceA>(this.Id, "ServiceA")
                let! s = proxy.Foo()
                count <- count + 1
                let msg = $"ServiceB.Foo(): actor={this.myId}, silo={appId}, count={count} -> {s}"
                Log.Information msg
                return msg
            }

    interface IServiceB with
        member this.Bar() =
            task {
                count <- count + 1
                let msg = $"ServiceB.Bar(): actor={this.myId}, silo={appId}, count={count}"
                Log.Information msg
                return msg
            }