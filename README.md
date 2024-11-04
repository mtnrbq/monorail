# README

This template scaffolds a web application with the following compnents configured:

#### Server

* Saturn
* Giraffe
* Fable.Remoting
* Dapr
* Fable.OpenTelemetry
* Tilt

#### Client

* Fable.Lit
* Fable.Remoting

## Prereqs

Start the Grafana LGTM container:

```sh
cd lgtm
./build.sh
./run.sh
```

## Using Dapr

This template inludes support for Dapr Actors. The custom `use_multiauth` and `use_oidc` authentication pipelines configure the user (principal) groups and roles (claims) via a `UserActor`, which can easily be migrated to an external Dapr service if need be.

Install the Dapr CLI and set up Dapr for use with actors:

```sh
dapr init -s
cp .dapr/components/* ~/.dapr/componets
```

Run the development server(s) under Dapr:

```sh
dapr run --app-id monorail --app-port 8085 -- dotnet run
```