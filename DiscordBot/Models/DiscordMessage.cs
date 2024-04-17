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

            if (message.Reference != null)
            {
                var refMessage = message.Channel.GetMessageAsync(message.Reference.MessageId.Value).Result;
                ReferenceAuthor = refMessage.Author.Username;
                ReferenceMessage = refMessage.Content;
            }
        }

        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ReferenceMessage { get; set; }
        public string ReferenceAuthor { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}