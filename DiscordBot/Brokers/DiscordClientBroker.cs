using Discord;
using Discord.WebSocket;

namespace DiscordBot.Brokers
{
    public class DiscordClientBroker : IDiscordClientBroker
    {
        private readonly DiscordSocketClient _client;

        public DiscordClientBroker()
        {
            _client = new DiscordSocketClient();
        }

        public event Func<LogMessage, Task> Log
        {
            add => _client.Log += value;
            remove => _client.Log -= value;
        }

        public event Func<Task> Ready
        {
            add => _client.Ready += value;
            remove => _client.Ready -= value;
        }

        public event Func<SocketMessage, Task> MessageReceived
        {
            add => _client.MessageReceived += value;
            remove => _client.MessageReceived -= value;
        }

        public async Task LoginAsync(TokenType tokenType, string token, bool validateToken = true)
            => await _client.LoginAsync(tokenType, token, validateToken);

        public async Task LogoutAsync() => await _client.LogoutAsync();
        public async Task StartAsync() => await _client.StartAsync();
        public async Task StopAsync() => await _client.StopAsync();
    }
}
