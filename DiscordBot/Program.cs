using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // new BotService().StartBotAsync().GetAwaiter().GetResult();

            var name = typeof(Program).Assembly.GetName().Name;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", name)
                .WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")    // only for local dev
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web application");
                CreateHostBuilder(args).Build().Run();
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
