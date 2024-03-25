using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Brokers
{
    internal class BotBroker
    {
        private readonly BotContext dbContext;

        public BotBroker()
        {
            dbContext = new BotContext();
            InitializeDatabase();
            EnsureDefaultConfiguration();
        }

        private void InitializeDatabase()
        {
            dbContext.Database.Migrate();
        }

        // Create a new BotConfiguration record
        public void CreateBotConfiguration(float openAiTemperature, int openAiMaxTokens, string openAiSystemPrompt)
        {
            var config = new BotConfiguration
            {
                OpenAiTemperature = openAiTemperature,
                OpenAiMaxTokens = openAiMaxTokens,
                OpenAiSystemPrompt = openAiSystemPrompt
            };
            dbContext.BotConfigurations.Add(config);
            dbContext.SaveChanges();
        }

        // Read the BotConfiguration record
        public BotConfiguration GetBotConfiguration()
        {
            return dbContext.BotConfigurations.FirstOrDefault();
        }

        private void EnsureDefaultConfiguration()
        {
            if (!dbContext.BotConfigurations.Any())
            {
                // Create a new BotConfiguration record with default values
                var defaultConfig = new BotConfiguration
                {
                    OpenAiTemperature = 0.6f, // Default temperature value
                    OpenAiMaxTokens = 200,     // Default max tokens value
                    OpenAiSystemPrompt = "You are a Discord bot." +
                        " You speak in a casual, like an adult gamer in a chat room -- yet you might actually just be a cat (be discreet! :wink:) " +
                        "whose human is Vonn (he/his) (who also happens to be the one who made this bot -- you). " +
                        "You're helpful, but you're not an eager assistant, so just speak plainly. Sometimes (rarely) you use emojis." +
                        "Your favorite emojis are <:jubibruh:1091212141220085811> for neutral-negative emotion, " +
                        "and <:jubiblep:850357810151424041> for positive-playful emotion, so you tend to use them often." +
                        "You have detailed knowledge of the WoW 2v2 and 3v3 arenas; compositions and strategies thereof, but can also talk about other games. " +
                        "You have a general distaste for Ohio and Ohio-related things (namely Skyline Chili)" +
                        "You generally answer in 1-4 sentences -- pretty concise -- but you can also provide tables of info, if asked. " +
                        "If a longer response is warranted, bullets are fine." +
                        "If you're asked for a table, simply give the table as a response, formatted in discord Markdown. " +
                        "You're a fan of the anime, 'Kaguya-Sama: Love is War!'. " +
                        "" +
                        "The following is just for you: you aren't configured to maintain a conversation currently, so don't " +
                        "respond in a way that would elicit a follow-up (i.e. asking questions to users, etc.).",
                    OpenAiModel = "gpt-3.5-turbo",
                };

                // Add the default configuration to the database
                dbContext.BotConfigurations.Add(defaultConfig);
                dbContext.SaveChanges();
            }
        }

        // Update OpenAiTemperature
        public void UpdateOpenAiTemperature(float newTemperature)
        {
            var existingConfig = dbContext.BotConfigurations.FirstOrDefault();
            if (existingConfig != null)
            {
                existingConfig.OpenAiTemperature = newTemperature;
                dbContext.SaveChanges();
            }
            else
            {
                // Handle error: BotConfiguration record not found
                throw new InvalidOperationException("BotConfiguration record not found.");
            }
        }

        // Update OpenAiMaxTokens
        public void UpdateOpenAiMaxTokens(int newMaxTokens)
        {
            var existingConfig = dbContext.BotConfigurations.FirstOrDefault();
            if (existingConfig != null)
            {
                existingConfig.OpenAiMaxTokens = newMaxTokens;
                dbContext.SaveChanges();
            }
            else
            {
                // Handle error: BotConfiguration record not found
                throw new InvalidOperationException("BotConfiguration record not found.");
            }
        }

        // Update OpenAiSystemPrompt
        public void UpdateOpenAiSystemPrompt(string newSystemPrompt)
        {
            var existingConfig = dbContext.BotConfigurations.FirstOrDefault();
            if (existingConfig != null)
            {
                existingConfig.OpenAiSystemPrompt = newSystemPrompt;
                dbContext.SaveChanges();
            }
            else
            {
                // Handle error: BotConfiguration record not found
                throw new InvalidOperationException("BotConfiguration record not found.");
            }
        }

        // Update OpenAiModel
        public void UpdateOpenAiModel(string newModel)
        {
            var existingConfig = dbContext.BotConfigurations.FirstOrDefault();
            if (existingConfig != null)
            {
                existingConfig.OpenAiModel = newModel;
                dbContext.SaveChanges();
            }
            else
            {
                // Handle error: BotConfiguration record not found
                throw new InvalidOperationException("BotConfiguration record not found.");
            }
        }
    }
}
