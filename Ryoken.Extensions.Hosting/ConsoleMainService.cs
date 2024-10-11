using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ryoken.Extensions.Hosting
{
    public sealed class ConsoleMainService : IHostedService
    {
        private readonly IConsoleMain _main;
        private readonly ILogger<ConsoleMainService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private Task? _mainTask;
        private int? _exitCode;

        public ConsoleMainService(IHostApplicationLifetime lifetime, IConsoleMain main, ILogger<ConsoleMainService> logger)
        {
            this._lifetime = lifetime;
            this._main = main;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken _)
        {
            _logger.LogDebug(nameof(StartAsync));

            // StartAsync happens during Application Start (aka before ApplicationStarted),
            // so register an action to run on ApplicationStarted.
            _lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    // we also need a slight delay, so all the Started message can flush before we actually start.
                    await Task.Delay(TimeSpan.FromSeconds(0.5));

                    // use a linked source based on the Stopping token, so _main can stop if the App signals
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);

                    // capture the main task, so we can wait for it to finish if App signals.
                    _mainTask = _main.ExecuteAsync(cts.Token);

                    // wait for the task to finish normally
                    await _mainTask.ConfigureAwait(false);

                    _logger.LogDebug("Main finished normally");
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
                    _logger.LogDebug("Calling StopApplication");
                    _lifetime.StopApplication();
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken _)
        {
            _logger.LogDebug(nameof(StopAsync));

            try
            {
                // wait for the main task to finish, up to 3 seconds
                if (_mainTask != null)
                    await _mainTask.WaitAsync(TimeSpan.FromSeconds(3));
            }
            catch (TimeoutException)
            {
                // swallow any timeout
                _logger.LogDebug("Main was cancelled, but didn't stop in time");
            }

            _logger.LogDebug("Bye!");

            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        }
    }
}