namespace DiscordBot.Models
{
    public class OpenAiResponse
    {
        public OpenAiResponseStatus Status { get; set; }
        public required string Content { get; set; }
    }
}
