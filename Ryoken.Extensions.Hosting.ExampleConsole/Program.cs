// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Ryoken.Extensions.Hosting;

Console.WriteLine("Hello, World!");

await Host.CreateDefaultBuilder(args)
          .ConfigureServices(services => services.AddConsoleMain<ConsoleMain>())
          .RunConsoleAsync();


internal class ConsoleMain : IConsoleMain
{
    public ConsoleMain(ILogger<ConsoleMain> logger)
    {
        Logger = logger;
    }

    public ILogger<ConsoleMain> Logger { get; }

    public async Task ExecuteAsync(CancellationToken token)
    {
        Logger.LogInformation("Hello From ConsoleMain");

        while (!token.IsCancellationRequested)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(1));

            Logger.LogInformation("Tick from ConsoleMain {tick}", DateTime.UtcNow);

            await timeout;
        }
    }
}