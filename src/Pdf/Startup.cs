using System;
using Backgrounding;
using Backgrounding.HostedServices;
using Backgrounding.Models;
using Backgrounding.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Policies;

namespace Pdf
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<HtmlWatcherOptions>(Configuration.GetSection(HtmlWatcherOptions.HtmlWatcher));

            services.AddScoped<HtmlReader>();
            services.AddScoped<PdfRequester>();
            services.AddScoped<HtmlProcessingService>();

            services.AddHttpClient<PdfRequester>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(HttpClientRetryPolicy.GetJitterRetryPolicy());

            var usePolling = Convert.ToBoolean(Configuration.GetSection($"{HtmlWatcherOptions.HtmlWatcher}:UsePolling").Value);

            if (usePolling)
            {
                services.AddHostedService<HtmlPollingBackgroundService>();
            }
            else
            {
                services.AddHostedService<HtmlWatcherBackgroundService>();
            }

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pdf", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pdf v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
