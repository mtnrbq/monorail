module Server.Api

open System.Net.Http.Json
open Dapr.Client
open Shared
open Dapr.Actors
open Dapr.Actors.Client
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FSharp.Control
open Giraffe
open Serilog

type Storage() =
    let notes = ResizeArray<_>() // no good, use mailboxprocessor instead

    member _.GetNotes() = Array.ofSeq notes |> Array.rev

    member _.AddNote(note: Note) =
        if Note.isValid note.Description then
            notes.Add note
            Ok note
        else
            Error "Invalid note"

let storage = Storage()

let noteApi: Api.NoteApi = {
    GetNotes = fun () -> async { return storage.GetNotes() }
    AddNote =
        fun msg ->
            let dapr = DaprClient.CreateInvokeHttpClient()
            let shoot () =
                task {
                    let! response = dapr.PostAsJsonAsync("http://monorail/shoot", "Tatooine")
                    let! result = response.Content.ReadAsStringAsync()
                    Log.Information $"shooter: {result}"
                    return result
                } |> Async.AwaitTask
            async {
                if msg = "Shoot!" then
                    let! target = shoot ()
                    let t = Note.create $"{msg}: {target}"
                    return storage.AddNote t
                else
                    let t = Note.create $"{msg}"
                    return storage.AddNote t
            }
}

let serviceApi: Api.ServiceApi =
    {
        InvokeA =
            fun aid ->
                Log.Debug "Invoking ServiceA()"
                task {
                    try
                        let proxy = ActorProxy.Create<Actors.IServiceA>(ActorId(aid), "ServiceA")
                        let! a = proxy.Foo()
                        Log.Debug a
                        return a
                    with exn ->
                        Log.Error $"%A{exn}"
                        return $"error: {exn.Message}"
                }
                |> Async.AwaitTask
        InvokeB =
            fun (aid, msg) ->
                Log.Debug $"Invoking ServiceB({msg})"
                task {
                    match msg with
                    | "A" ->
                        let proxy = ActorProxy.Create<Actors.IServiceA>(ActorId(aid), "ServiceB")
                        return! proxy.Foo()
                    | _ ->
                        let proxy = ActorProxy.Create<Actors.IServiceB>(ActorId(aid), "ServiceB")
                        return! proxy.Bar()
                }
                |> Async.AwaitTask
    }

let noteEndpoints: HttpHandler =
    Remoting.createApi ()
    |> Remoting.fromValue noteApi
    |> Remoting.withRouteBuilder Api.routeBuilder
    |> Remoting.buildHttpHandler

let serviceEndpoints: HttpHandler =
    Remoting.createApi ()
    |> Remoting.fromValue serviceApi
    |> Remoting.withRouteBuilder Api.routeBuilder
    |> Remoting.buildHttpHandler