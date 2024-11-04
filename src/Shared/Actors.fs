module Shared.Actors

open Dapr.Actors
open System.Threading.Tasks

let appId =
    let dapr = Dapr.Client.DaprClientBuilder().Build()
    task {
        let! m = dapr.GetMetadataAsync()
        return m.Id
    } |> Async.AwaitTask |> Async.RunSynchronously

type Group = string

type IUserActor =
    inherit IActor
    abstract GetGroups: unit: unit -> Task<string[]>
    abstract GetRoles: unit: unit -> Task<string[]>

type IActorA =
    inherit IActor
    abstract Foo: unit: unit -> Task<string>

type IActorB =
    inherit IActor
    abstract Bar: unit: unit -> Task<string>