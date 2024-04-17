using Discord.WebSocket;

namespace DiscordBot.Models
{
    public class DiscordMessage
    {
        public DiscordMessage() { }

        public DiscordMessage(SocketMessage message)
        {
            Username = message.Author.Username;
            Message = message.Content;
            Timestamp = message.Timestamp;
        }

        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}