using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ryoken.Extensions.Hosting
{
    public sealed partial class ConsoleMainService : IHostedService
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

        [LoggerMessage(LogLevel.Debug, "Start event detected, setting up Main")]
        static partial void LogStart(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Main is starting")]
        static partial void LogMainIsStarting(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Main finished normally")]
        static partial void LogMainFinishedNormally(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Main was cancelled")]
        static partial void LogMainCancelled(ILogger logger);

        [LoggerMessage(LogLevel.Critical, "Unhandled Exception in Main")]
        static partial void LogUnhandledInMain(ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Debug, "Main is finished, calling StopApplication")]
        static partial void LogMainFinished(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Stop event detected, waiting for main to finish")]
        static partial void LogStop(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Main was cancelled, but didn't stop in time")]
        static partial void LogMainDidNotStop(ILogger logger);

        [LoggerMessage(LogLevel.Debug, "Stopping, Bye!")]
        static partial void LogAllDone(ILogger logger);

        public Task StartAsync(CancellationToken _)
        {
            LogStart(_logger);

            // StartAsync happens during Application Start (aka before ApplicationStarted),
            // so register an action to run on ApplicationStarted.
            _lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    // we also need a slight delay, so all the Started message can flush before we actually start.
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    LogMainIsStarting(_logger);

                    // use a linked source based on the Stopping token, so _main can stop if the App signals
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);

                    // capture the main task, so we can wait for it to finish if App signals.
                    _mainTask = _main.ExecuteAsync(cts.Token);

                    // wait for the task to finish normally
                    await _mainTask.ConfigureAwait(false);

                    LogMainFinishedNormally(_logger);
                    // when _main finishes, then signal that the app can finish.
                    _exitCode = 0;
                }
                // absorb TaskCancelledException.
                catch (TaskCanceledException)
                {
                    LogMainCancelled(_logger);
                }
                catch (Exception ex)
                {
                    LogUnhandledInMain(_logger, ex);
                    _exitCode = 1;
                }
                finally
                {
                    LogMainFinished(_logger);
                    _lifetime.StopApplication();
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken _)
        {
            LogStop(_logger);

            try
            {
                // wait for the main task to finish, up to 3 seconds
                // unless we already caught an exception from main
                var timeout = TimeSpan.FromSeconds(3);
                if (_mainTask != null && _exitCode != 1)
                    await _mainTask.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                LogMainDidNotStop(_logger);
            }

            LogAllDone(_logger);

            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
        }
    }
}