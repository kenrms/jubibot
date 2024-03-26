using Azure;
using Azure.AI.OpenAI;
using Discord;
using Discord.WebSocket;
using DiscordBot.Brokers;
using DiscordBot.Models;

namespace DiscordBot
{
    public class BotService : IBotService
    {
        private readonly string discordToken;
        private readonly string openAiKey;
        private readonly BotBroker botBroker;
        private readonly DiscordSocketClient _client;
        private bool _isRunning;

        public float openAiTemp { get; private set; }
        public int openAiMaxTokens { get; private set; }
        public string openAiSystemPrompt { get; private set; }
        public string openAiModel { get; private set; }

        public bool IsRunning() => _isRunning;
        public float GetTemperature() => botBroker.GetBotConfiguration().OpenAiTemperature;

        public BotService()
        {
            botBroker = new BotBroker();
            InitializeConfiguration();
            discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");

            if (string.IsNullOrEmpty(discordToken) || string.IsNullOrEmpty(openAiKey))
            {
                throw new InvalidOperationException("Discord token or OpenAI key is missing from environment variables.");
            }

            _client = new DiscordSocketClient();
            _client.Log += LogToConsoleAsync;
            _client.MessageReceived += MessageHandler;
            _isRunning = false;
        }

        private void InitializeConfiguration()
        {
            BotConfiguration botConfig = botBroker.GetBotConfiguration();
            openAiMaxTokens = botConfig.OpenAiMaxTokens;
            openAiTemp = botConfig.OpenAiTemperature;
            openAiSystemPrompt = botConfig.OpenAiSystemPrompt;
            openAiModel = botConfig.OpenAiModel;
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
                DeploymentName = openAiModel,
                Messages =
                {
                    new ChatRequestSystemMessage(openAiSystemPrompt),
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

        public async Task StartBotAsync()
        {
            if (!_isRunning)
            {
                await _client.LoginAsync(TokenType.Bot, discordToken);
                await _client.StartAsync();
                _isRunning = true;
            }
        }

        public async Task StopBotAsync()
        {
            if (_isRunning)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                _isRunning = false;
            }
        }

        public void SetTemperature(float newTemp)
        {
            openAiTemp = newTemp;
            botBroker.UpdateOpenAiTemperature(newTemp);
        }

        public void SetOpenAiMaxTokens(int maxTokens)
        {
            openAiMaxTokens = maxTokens;
            botBroker.UpdateOpenAiMaxTokens(maxTokens);
        }

        public void SetOpenAiModel(string model)
        {
            openAiModel = model;
            botBroker.UpdateOpenAiModel(model);
        }

        public void SetOpenAiSystemPrompt(string systemPrompt)
        {
            openAiSystemPrompt = systemPrompt;
            botBroker.UpdateOpenAiSystemPrompt(systemPrompt);
        }
    }
}
