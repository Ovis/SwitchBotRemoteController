using Microsoft.Extensions.DependencyInjection;

namespace SwitchBotRemoteController.Extensions
{
    public static class SwitchBotClientExtensions
    {
        public static IServiceCollection AddSwitchBotClient(this IServiceCollection services)
        {
            services.AddSingleton<SwitchBotClient>();

            return services;
        }
    }
}
