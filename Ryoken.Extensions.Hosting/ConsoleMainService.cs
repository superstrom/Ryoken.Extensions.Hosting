using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ryoken.Extensions.Hosting
{
    public sealed class ConsoleMainService : IHostedService
    {
        private readonly IConsoleMain _main;
        private readonly ILogger<ConsoleMainService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private int? _exitCode;

        public ConsoleMainService(IHostApplicationLifetime lifetime, IConsoleMain main, ILogger<ConsoleMainService> logger)
        {
            this._lifetime = lifetime;
            this._main = main;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken tok)
        {
            _logger.LogDebug("StartAsync");

            // StartAsync happens during Application Start (aka before ApplicationStarted),
            // so register an action to run on ApplicationStarted.
            _lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    // we also need a slight delay, so all the Started message can flush before we actually start.
                    await Task.Delay(TimeSpan.FromSeconds(0.5));

                    // pass the Stopping token, so _main can stop if the App signals
                    await _main.ExecuteAsync(_lifetime.ApplicationStopping).ConfigureAwait(false);

                    // when _main finishes, then signal that the app can finish.
                    _exitCode = 0;
                }
                // absorb TaskCancelledException.
                catch (TaskCanceledException)
                {
                    _logger.LogDebug("Task Cancelled");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unhandled");
                    _exitCode = 1;
                }
                finally
                {
                    _logger.LogDebug("Stopping");
                    _lifetime.StopApplication();
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            _logger.LogDebug(nameof(StopAsync));

            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);

            return Task.CompletedTask;
        }
    }
}