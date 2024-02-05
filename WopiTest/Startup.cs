using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WopiCore.Services;

namespace WopiTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IConfiguration configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            WopiCore.Services.WopiStartup.ConfigureServices(services);
            services.AddSingleton<WopiCore.Services.IWopiConfiguration, WopiConfiguration>();
            services.AddScoped<IWopiDbRepository, WopiDbRepository>();
            services.AddScoped<IWopiStorageRepository, WopiStorageRepository>();

            services.AddLogging();
            services.AddControllers().AddApplicationPart(typeof(WopiCore.Services.WopiStartup).Assembly);
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseMiddleware<RequestLoggingMiddleware>();
            // app.UseHttpsRedirection();

            // TODO: Fix for production
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
