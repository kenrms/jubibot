using Amazon.DynamoDBv2;
using DiscordBot.Brokers;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
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
                .WriteTo.Seq(serverUrl: "http://host.docker.internal:5341")    // only for local dev
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting console application");
                var builder = CreateHostBuilder(args);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ICustomConfiguration, CustomConfiguration>();
                    services.AddSingleton<IBotBroker, DynamoDBBotBroker>();
                    services.AddSingleton<IDiscordClientBroker, DiscordClientBroker>();
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

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    Amazon.Extensions.NETCore.Setup.AWSOptions awsOptions = hostContext.Configuration.GetAWSOptions();
                    Console.WriteLine($"BOT | AWS Region: {awsOptions.Region}");
                    Console.WriteLine($"BOT | AWS Profile: {awsOptions.Profile}");
                    services.AddDefaultAWSOptions(hostContext.Configuration.GetAWSOptions());
                    services.AddAWSService<IAmazonDynamoDB>();
                    services.AddHostedService<BotService>();
                })
                .UseSerilog();
        }
    }
}
