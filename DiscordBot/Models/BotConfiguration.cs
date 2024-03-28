namespace DiscordBot.Models
{
    public class BotConfiguration
    {
        public int Id { get; set; }
        public float OpenAiTemperature { get; set; }
        public int OpenAiMaxTokens { get; set; }
        public string OpenAiSystemPrompt { get; set; }
        public string OpenAiModel { get; set; }
    }

}
