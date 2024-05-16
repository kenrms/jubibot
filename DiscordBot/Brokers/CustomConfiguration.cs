using Microsoft.Extensions.Configuration;

namespace DiscordBot.Brokers
{
    public class CustomConfiguration : ICustomConfiguration
    {
        private readonly IConfiguration _config;

        public CustomConfiguration(IConfiguration config)
        {
            _config = config;
        }

        public string GetOpenAiKey()
        {
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);

            return isDevelopment ? _config["OpenAiKey"] : Environment.GetEnvironmentVariable("OPENAI_KEY");
        }

        public string GetDiscordToken()
        {
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);

            return isDevelopment ? _config["DiscordToken"] : Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        }
    }

}
