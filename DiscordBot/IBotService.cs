namespace DiscordBot
{
    public interface IBotService
    {
        float openAiTemp { get; }
        int openAiMaxTokens { get; }
        string openAiSystemPrompt { get; }
        string openAiModel { get; }

        bool IsRunning();
        Task StartBotAsync();
        Task StopBotAsync();
        void SetTemperature(float newTemp);
        void SetOpenAiMaxTokens(int maxTokens);
        void SetOpenAiModel(string model);
        void SetOpenAiSystemPrompt(string systemPrompt);
    }
}
