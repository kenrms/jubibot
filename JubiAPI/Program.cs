using DiscordBot;
using JubiAPI.Middleware;
using Serilog;

namespace JubiAPI
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
                Log.Information("Starting web application");
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();
                builder.WebHost.UseKestrel();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAnyOrigin",
                        policy =>
                        {
                            policy.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                        });
                });

                // Add services to the container.
                builder.Services.AddSingleton<IBotService, BotService>();

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                WebApplication app = builder.Build();
                app.UseMiddleware<CustomExceptionHandlingMiddleware>();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseCors("AllowAnyOrigin");
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
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
    }
}
