namespace DiscordBot.Brokers
{
    public interface ICustomConfiguration
    {
        string GetOpenAiKey();
        string GetDiscordToken();
    }
}
