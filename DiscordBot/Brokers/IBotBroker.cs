using DiscordBot.Models;

namespace DiscordBot.Brokers
{
    public interface IBotBroker
    {
        public Task<BotConfiguration> GetBotConfiguration();
        public Task UpdateOpenAiTemperature(float newTemperature);
        public Task UpdateOpenAiMaxTokens(int newMaxTokens);
        public Task UpdateOpenAiModel(string newModel);
        public Task UpdateOpenAiSystemPrompt(string newSystemPrompt);
    }
}
