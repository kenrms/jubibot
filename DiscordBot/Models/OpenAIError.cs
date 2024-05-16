namespace DiscordBot.Models
{
    public class OpenAIError
    {
        public required OpenAIErrorInner Error { get; set; }

        public class OpenAIErrorInner
        {
            public required string Message { get; set; }
            public required string Type { get; set; }
        }
    }
}
