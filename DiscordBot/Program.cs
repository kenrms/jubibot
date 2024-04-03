using DiscordBot.Brokers;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var name = typeof(Program).Assembly.GetName().Name;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", name)
                //.WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")    // only for local dev
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting console application");
                var builder = CreateHostBuilder(args);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IBotBroker, BotEfBroker>();
                });

                builder.Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<BotService>();
                })
                .UseSerilog();
    }
}
