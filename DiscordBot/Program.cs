namespace DiscordBot
{
    internal class Program
    {
        static void Main(string[] args) =>
            new BotService().StartBotAsync().GetAwaiter().GetResult();
    }
}
