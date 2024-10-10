namespace Ryoken.Extensions.Hosting
{
    public interface IConsoleMain
    {
        Task ExecuteAsync(CancellationToken token);
    }
}