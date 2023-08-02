using AutoFAQ.Application.Services;
using AutoFAQ.Core.Extensions;
using AutoFAQ.Database.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoFAQ.Bot {
    internal class Program {
        static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args) {
            var host = Host.CreateDefaultBuilder(args)
                .AddBotCraft()
                .ConfigureServices(ConfigureServices);

            await host.RunConsoleAsync();
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services) {
            services.AddTransient<AutoFAQService>();
            services.AddTransient<GuildConfigService>();
            services.AddHostedService<AutoFAQReplyService>();
        }
    }
}