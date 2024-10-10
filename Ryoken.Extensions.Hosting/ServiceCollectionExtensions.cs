using Microsoft.Extensions.DependencyInjection;

using Ryoken.Extensions.Hosting;

namespace Microsoft.Extensions.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static void AddConsoleMain<T>(this IServiceCollection services) where T : class, IConsoleMain
        {
            services.AddSingleton<IConsoleMain, T>();
            services.AddHostedService<ConsoleMainService>();
        }
    }
}