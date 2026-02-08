// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Ryoken.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
          .ConfigureServices(services => services.AddConsoleMain<ConsoleMain>())
          .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Debug))
          .RunConsoleAsync();

class ConsoleMain(ILogger<ConsoleMain> Logger) : IConsoleMain
{
    public async Task ExecuteAsync(CancellationToken token)
    {
        Logger.LogInformation("Hello from ConsoleMain!");

        for (int i = 0; i < 10 && !token.IsCancellationRequested; ++i)
        {
            var timeout = Task.Delay(TimeSpan.FromSeconds(1));

            Logger.LogInformation("Tick from ConsoleMain {tick}", DateTime.UtcNow);

            await timeout;
        }

        if (!token.IsCancellationRequested)
            throw new ApplicationException("Unhandled error in ConsoleMain");

        Logger.LogInformation("Done!");
    }
}