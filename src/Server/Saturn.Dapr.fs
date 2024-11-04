module Saturn.Dapr

open System.Text.Json
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Saturn
open Dapr.Actors.Runtime
open System.Text.Json.Serialization

type ApplicationBuilder with
    [<CustomOperation("use_dapr")>]
    member this.UseDapr(state, configureActors: ActorRuntimeOptions -> unit) =
        let middleware (app: IApplicationBuilder) =
            app
                .UseCloudEvents()
                .UseRouting()
                .UseEndpoints(fun ep ->
                    ep.MapActorsHandlers() |> ignore
                    ep.MapSubscribeHandler() |> ignore)

        let service (service: IServiceCollection) =
            let jconv = JsonFSharpConverter(JsonFSharpOptions.ThothLike())
            let jopt = JsonSerializerOptions(JsonSerializerDefaults.Web)
            jopt.Converters.Add(jconv)
            service.AddDaprClient(fun builder ->
                builder.UseJsonSerializationOptions(jopt)
                |> ignore)
            service
                .AddControllers()
                .AddJsonOptions(fun jsonOptions -> jsonOptions.JsonSerializerOptions.Converters.Add(jconv))
                .AddDapr(fun builder ->
                    builder.UseJsonSerializationOptions(jopt)
                    |> ignore)
            |> ignore
            service.AddActors(fun o ->
                o.JsonSerializerOptions <- jopt
                configureActors o)
            service

        this.ServiceConfig(state, service)
        |> fun state -> this.AppConfig(state, middleware)