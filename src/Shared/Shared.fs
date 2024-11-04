namespace Shared

open System

type Note = { Id: Guid; Description: string }

type Svc =
    | A
    | B of string

module Api =
    let routeBuilder (typeName: string) (methodName: string) =
            $"/api/v1/{typeName}/{methodName}"

    type NoteApi =
        {
            GetNotes: unit -> Async<Note []>
            AddNote: string -> Async<Result<Note, string> >
        }

    type ShootApi =
        {
            shoot: string -> Async<Result<Note, string> >
        }

    type ServiceApi =
        {
            InvokeA: string -> Async<string>
            InvokeB: string * string -> Async<string>
        }

module Note =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid(); Description = description }