module Settings

open System.IO
open Thoth.Json.Net
open Serilog

type Settings = {
    appEnv: string option
    appName: string option
    appNamespace: string option
    appVersion: string option
    otelCollector: string
}

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
        | null
        | "" -> None
        | x -> Some x

let appsettings =
    let settings =
        File.ReadAllText "appsettings.json"
        |> Decode.Auto.fromString<Settings>
        |> function
            | Ok s -> s
            | Error e -> failwith e
    {
        settings with
            appEnv = Some "devel"
            appName = Some "monorail"
            appNamespace = Some "spmsa"
            appVersion = Some "1.0.0"
    }

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map int
    |> Option.defaultValue 8085

sprintf "Settings: %A" appsettings |> Log.Debug