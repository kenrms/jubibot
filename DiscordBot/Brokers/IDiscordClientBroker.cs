using Discord;
using Discord.WebSocket;

namespace DiscordBot.Brokers
{
    public interface IDiscordClientBroker
    {
        public event Func<LogMessage, Task> Log;
        public event Func<Task> Ready;
        public event Func<SocketMessage, Task> MessageReceived;
        public Task LoginAsync(TokenType tokenType, string token, bool validateToken = true);
        public Task LogoutAsync();
        public Task StartAsync();
        public Task StopAsync();
    }
}
