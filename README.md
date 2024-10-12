# Ryoken.Extensions.Hosting

[![.NET](https://github.com/superstrom/Ryoken.Extensions.Hosting/actions/workflows/dotnet.yml/badge.svg)](https://github.com/superstrom/Ryoken.Extensions.Hosting/actions/workflows/dotnet.yml)
![NuGet Version](https://img.shields.io/nuget/v/Ryoken.Extensions.Hosting)

Ever wanted to have a simple Console app that terminates when done, but still has access to DI and all other the benefits of Microsoft.Extensions.Hosting?
This library allows you to easily implement a single function and handles the ApplicationLifetime interaction.

Based on https://dfederm.com/building-a-console-app-with-.net-generic-host/

## Minimal Example
```csharp
await Host.CreateDefaultBuilder(args)
          .ConfigureServices(services => services.AddConsoleMain<ConsoleMain>())
          .RunConsoleAsync();

class ConsoleMain : IConsoleMain
{
    public ConsoleMain(/* Inject your services here */){}

    public async Task ExecuteAsync(CancellationToken token)
    {
        Logger.LogInformation("Hello World!");
    }
}
```
