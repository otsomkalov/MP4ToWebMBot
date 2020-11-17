using FFmpeg.NET;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MP4ToWebMBot.Services;
using MP4ToWebMBot.Settings;
using Telegram.Bot;

namespace MP4ToWebMBot
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = _configuration.Get<AppSettings>();

            services
                .AddSingleton<ITelegramBotClient>(new TelegramBotClient(settings.Telegram.Token))
                .AddSingleton(new Engine(settings.FFMpeg.Path))
                .AddSingleton(settings)
                .AddTransient<IMessageService, MessageService>();

            services.AddHealthChecks();
            
            services.AddApplicationInsightsTelemetry();
            
            services.AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles("/telegram-bot-api-data");
            
            app.UseRouting();

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}