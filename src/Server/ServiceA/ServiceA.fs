module ServiceA

open Dapr.Actors.Runtime
open Serilog

open Shared.Actors

type ServiceA(host: ActorHost) =
    inherit Actor(host)

    let mutable count = 0
    member this.myId = this.Id.GetId()

    interface IServiceA with
        member this.Foo() =
            task {
                count <- count + 1
                let msg = $"ServiceA.Foo: actor={this.myId}, silo={appId}, count={count}"
                Log.Information msg
                return msg
            }