using DiscordBot.Models;

namespace DiscordBot.Brokers
{
    public interface IBotBroker
    {
        public BotConfiguration GetBotConfiguration();
        public void UpdateOpenAiTemperature(float newTemperature);
        public void UpdateOpenAiMaxTokens(int newMaxTokens);
        public void UpdateOpenAiModel(string newModel);
        public void UpdateOpenAiSystemPrompt(string newSystemPrompt);
    }
}
