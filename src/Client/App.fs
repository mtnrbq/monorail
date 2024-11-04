module App

open Lit
open Lit.Elmish
open Fable.Remoting.Client
open Fable.Core

open Shared
open Contexts

JsInterop.importSideEffects "./public/style.scss"

type Model = {
    notes: string list
    notification: string option
}

type Msg =
    | GetNotes
    | SetNotes of string array
    | AddNote of string
    | Notify of string
    | Reset of unit
    | Invoke of string * Svc
    | Ignore

[<Emit("btoa($0)")>]
let toBase64String (_: string) : string = jsNative

let notesApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Api.routeBuilder
    |> Remoting.buildProxy<Api.NoteApi>

let serviceApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Api.routeBuilder
    |> Remoting.buildProxy<Api.ServiceApi>

let init () =
    {
        notes = []
        notification = None
    }, Elmish.Cmd.OfAsync.perform notesApi.GetNotes () (fun x -> x |> Array.map _.Description |> SetNotes)

let update (msg: Msg) (model: Model) =
    match msg with
    | GetNotes ->
        let handler (t: Note []) =
            let notes = Array.map _.Description t
            SetNotes notes
        model, Elmish.Cmd.OfAsync.perform notesApi.GetNotes () handler
    | SetNotes notes ->
        let m = { model with notes = List.ofArray notes }
        m, Elmish.Cmd.ofMsg (Notify "Fetched notes.")
    | AddNote note ->
        let m = { model with notes = note :: model.notes }
        m, Elmish.Cmd.OfAsync.perform notesApi.AddNote note (fun _ -> Ignore)
    | Notify x ->
        let action () =
            async {
                do! Async.Sleep 2500
                return ()
            }
        let m = { model with notification = if x.Length = 0 then None else Some x }
        m, Elmish.Cmd.OfAsync.perform action () Reset
    | Reset _ ->
        let m =
           { model with
                notification = None
           }
        m, Elmish.Cmd.none
    | Invoke (aid, svc) ->
        let s =
            match svc with
            | A -> serviceApi.InvokeA
            | B x -> fun aid -> serviceApi.InvokeB (aid, string x)
        model, Elmish.Cmd.OfAsync.perform s aid AddNote
    | Ignore -> model, Elmish.Cmd.none

let private hmr = HMR.createToken ()

let listNotes (notes: string list) =
    let ns = notes |> List.map (fun note -> html $"<li>{note}</li>")
    html $"<ul>{ns}</ul>"

let memoInit () =
    { memo = "" }, Elmish.Cmd.none

let memoUpdate (msg: MemoMsg) (model: Memo) =
    match msg with
    | SetMemo s -> { model with memo = s }, Elmish.Cmd.none

[<LitElement("web-app")>]
let InitApp () =
    Hook.useHmr hmr
    LitElement.init (fun cfg -> cfg.useShadowDom <- false) |> ignore

    let model, dispatch = Hook.useElmish (init, update)

    let foo, fooDispatch = Hook.useElmish (memoInit, memoUpdate)
    Hook.addProvider(memoCtx, (foo, fooDispatch)) |> ignore

    html $"""
        <div class="full-width-center notification-bar">
          <sl-alert ?open={model.notification.IsSome}>
            <sl-icon slot="icon" name="info-circle"></sl-icon>
            {Option.defaultValue "" model.notification}
          </sl-alert>
        </div>
        <sl-split-panel position-in-pixels="350">
        <div slot="start" class="full-width-center" style="padding: 15px;">
          <h3>Death star</h3>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (AddNote "Shoot!"))}>Shoot!
          </sl-button>
          <h4>Darth</h4>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("darth", A)))}>
            Actor A
          </sl-button>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("darth", B "A")))}>
            Actor B (A)
          </sl-button>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("darth", B "B")))}>
            Actor B (B)
          </sl-button>
          <h4>Obi-Wan</h4>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("obi-wan", A)))}>
            Actor A
          </sl-button>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("obi-wan", B "A")))}>
            Actor B (A)
          </sl-button>
          <sl-button
            style="padding: 10px;"
            @click={Ev(fun _ -> dispatch (Invoke ("obi-wan", B "B")))}>
            Actor B (B)
          </sl-button>
        </div>
        <div slot="end" class="full-width-center">
          {listNotes model.notes}
        </div>
        </sl-split-panel>
    """