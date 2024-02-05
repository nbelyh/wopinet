using Microsoft.Extensions.DependencyInjection;

namespace WopiCore.Services
{
    public static class WopiStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            services.AddSingleton<WopiDiscovery>();
            services.AddScoped<WopiSecurity>();
            services.AddScoped<IWopiFileRepository, WopiFileRepository>();
        }
    }
}
