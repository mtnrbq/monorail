module Server.Main

open System
open Argu
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Dapr.Actors
open Giraffe
open Saturn
open Saturn.Dapr
open Saturn.OpenTelemetry
open Serilog
open Serilog.Events
open Serilog.Sinks.OpenTelemetry
open Settings

// START PREAMBLE BOILERPLATE
type Arguments =
    | Log_Level of level: int
    | Port of port: int
    | [<MainCommand; Last>] Dir of path: string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Log_Level _ -> "0=Error, 1=Warning, 2=Info, 3=Debug, 4=Verbose"
            | Port _ -> "listen port (default 8085)"
            | Dir _ -> "serve from dir"

let colorizer =
    function
    | ErrorCode.HelpText -> None
    | _ -> Some ConsoleColor.Red

let errorHandler = ProcessExiter(colorizer = colorizer)

let configureSerilog level =
    let n =
        match level with
        | 0 -> LogEventLevel.Error
        | 1 -> LogEventLevel.Warning
        | 2 -> LogEventLevel.Information
        | 3 -> LogEventLevel.Debug
        | _ -> LogEventLevel.Verbose
    LoggerConfiguration()
        .MinimumLevel.Is(n)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Information)
        .Filter.ByExcluding("RequestPath like '/health%'")
        .WriteTo.Console()
        .WriteTo.OpenTelemetry(fun opt ->
            opt.Endpoint <- appsettings.otelCollector
            opt.IncludedData <-
                IncludedData.TraceIdField
                ||| IncludedData.SpanIdField
                ||| IncludedData.SourceContextAttribute
            opt.ResourceAttributes <-
                dict [
                    "service.name", box $"monorail-{appsettings.appEnv.Value}"
                    "service.namespace", box "default"
                    "host.name", box Environment.MachineName
                    "service.version", box appsettings.appVersion.Value
                ])
        .Enrich.FromLogContext()
        .CreateLogger()

let configureServices (services: IServiceCollection) =
    services.AddLogging(fun (b: ILoggingBuilder) -> b.ClearProviders().AddSerilog() |> ignore)

let otelConfig = {
    AppId = appsettings.appName.Value
    Namespace = appsettings.appNamespace.Value
    Version = appsettings.appVersion.Value
    Endpoint = appsettings.otelCollector
}
// END PREAMBLE

let configureActors (options: Runtime.ActorRuntimeOptions) =
    options.Actors.RegisterActor<ServiceA.ServiceA>()
    options.Actors.RegisterActor<ServiceB.ServiceB>()
    options.ActorIdleTimeout <- TimeSpan.FromSeconds 60.0

let shooter (next: HttpFunc) (ctx: HttpContext) =
       task {
           let! payload = ctx.BindJsonAsync<string>()
           Log.Debug $"shoot for {payload}"
           return! json {| target = payload |} next ctx
       }

let webApp =
    choose [
        routeStartsWith "/shoot" >=> shooter
        routeStartsWith "/api/v1/ServiceApi/" >=> Api.serviceEndpoints
        routex "/api/v1/NoteApi/.*" >=> Api.noteEndpoints
    ]

let app port =
    application {
        url $"http://0.0.0.0:{port}"
        use_otel (
            configure_otel {
                settings otelConfig
                use_redis
            }
        )
        use_router webApp
        use_dapr configureActors
        service_config configureServices
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
        use_gzip
        memory_cache
        logging (fun logger -> logger.AddSerilog() |> ignore)
    }

[<EntryPoint>]
let main argv =
    let parser =
        ArgumentParser.Create<Arguments>(programName = "Monorail", errorHandler = errorHandler)
    let args = parser.Parse argv
    let port = args.GetResult(Port, defaultValue = Settings.port)
    Log.Logger <- configureSerilog (args.GetResult(Log_Level, defaultValue = 4))
    run (app port)
    0