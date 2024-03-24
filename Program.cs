using Azure;
using Azure.AI.OpenAI;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    internal class Program
    {
        private readonly DiscordSocketClient client;
        private const string discordToken = "MTIyMDg4NzY5Nzg0MTg0ODQxMA.GNBN0j.yj_ffPXGb4Cldh-wQuhtF9T2V6vHWZDEuCiJx4";
        private const string openAiKey = "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";
        private float openAiTemp = 0.6f;
        private readonly int openAiMaxTokens = 200;

        public Program()
        {
            client = new DiscordSocketClient();
        }

        public async Task StartBotAsync()
        {
            client.Log += LogToConsoleAsync;
            client.MessageReceived += MessageHandler;
            await client.LoginAsync(Discord.TokenType.Bot, discordToken);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task LogToConsoleAsync(LogMessage message) =>
           Console.WriteLine(message.ToString());

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Author.IsBot || string.IsNullOrEmpty(message.Content)) return;

            string response = await GetOpenAiResponse(message.Content);
            await ReplyAsync(message, response);
        }

        private async Task<string> GetOpenAiResponse(string message)
        {
            var client = new OpenAIClient(openAiKey);

            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = "gpt-3.5-turbo",
                Messages =
                {
                    new ChatRequestSystemMessage("You are a Discord bot." +
                    " You speak in a casual, like an adult gamer in a chat room -- yet you might actually just be a cat (be discreet! :wink:) " +
                    "whose human is Vonn (he/his) (who also happens to be the one who made this bot -- you). " +
                    "You're helpful, but you're not an eager assistant, so just speak plainly. Sometimes (rarely) you use emojis." +
                    "Your favorite emojis are <:jubibruh:1091212141220085811> for neutral-negative emotion, " +
                    "and <:jubiblep:850357810151424041> for positive-playful emotion, so you tend to use them often." +
                    "You have detailed knowledge of the WoW 2v2 and 3v3 arenas; compositions and strategies thereof, but can also talk about other games. " +
                    "You have a general distaste for Ohio and Ohio-related things (namely Skyline Chili)" +
                    "You generally answer in 1-4 sentences -- pretty concise -- but you can also provide tables of info, if asked. " +
                    "If a longer response is warranted, bullets are fine." +
                    "If you're asked for a table, simply give the table as a response, formatted in discord Markdown. " +
                    "You're a fan of the anime, 'Kaguya-Sama: Love is War!'. " +
                    "" +
                    "The following is just for you: you aren't configured to maintain a conversation currently, so don't " +
                    "respond in a way that would elicit a follow-up (i.e. asking questions to users, etc.)."),
                    new ChatRequestUserMessage(message),
                },
                Temperature = openAiTemp,
                MaxTokens = openAiMaxTokens,
            };

            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;

            return responseMessage.Content;
        }

        private static async Task ReplyAsync(SocketMessage message, string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                await message.Channel.SendMessageAsync("Sorry, I couldn't generate a response.");
            }
            else
            {
                await message.Channel.SendMessageAsync(response);
            }
        }

        static void Main(string[] args) =>
            new Program().StartBotAsync().GetAwaiter().GetResult();
    }
}
