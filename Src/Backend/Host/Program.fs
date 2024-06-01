module MusicPlayerBackend.Host.Program

open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Hosting
open Serilog
open Microsoft.Extensions.Configuration

open MusicPlayerBackend.Common

let createHostBuilder args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun b ->  %b.UseStartup<Startup>())
        .ConfigureAppConfiguration(fun b -> %b.AddEnvironmentVariables(prefix = "mservice__"))

[<EntryPoint>]
let main args =
    Log.Logger <- LoggerConfiguration().WriteTo.Console().CreateLogger()

    createHostBuilder(args)
        .Build()
        .Run()
    0
