using Azure;
using Azure.AI.OpenAI;
using Discord;
using Discord.WebSocket;
using DiscordBot.Brokers;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiscordBot
{
    public class BotService : BackgroundService, IBotService
    {
        private readonly string discordToken;
        private readonly string openAiKey;
        private readonly BotBroker botBroker;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private bool _isRunning;

        public float openAiTemp { get; private set; }
        public int openAiMaxTokens { get; private set; }
        public string openAiSystemPrompt { get; private set; }
        public string openAiModel { get; private set; }

        public bool IsRunning() => _isRunning;
        public float GetTemperature() => botBroker.GetBotConfiguration().OpenAiTemperature;

        public BotService(IConfiguration config)
        {
            botBroker = new BotBroker();
            InitializeConfiguration();


            _config = config;
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);

            if (isDevelopment)
            {
                discordToken = _config["DiscordToken"];
                openAiKey = _config["OpenAiKey"];
            }
            else
            {
                discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
            }

            _client = new DiscordSocketClient();

            InitializeClient();
        }

        private void InitializeClient()
        {
            _client.Log += HandleDiscordClientLogging;
            _client.MessageReceived += MessageHandler;
            _client.Ready += ReadyAsync;
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

        private Task HandleDiscordClientLogging(LogMessage message)
        {
            Log.Information(message.ToString());

            return Task.CompletedTask;
        }

        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Author.IsBot || string.IsNullOrEmpty(message.Content)) return;

            string response = await GetOpenAiResponse(message.Content);
            await ReplyAsync(message, response);
        }

        private async Task<string> GetOpenAiResponse(string message)
        {
            if (string.IsNullOrEmpty(openAiKey))
            {
                throw new Exception($"Invalid OpenAI key provided: {openAiKey}");
            }

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
                if (string.IsNullOrEmpty(discordToken))
                {
                    throw new Exception($"Invalid Discord Token provided: {discordToken}");
                }

                await _client.LoginAsync(TokenType.Bot, discordToken);
                await _client.StartAsync();
                _isRunning = true;
                Log.Information("Bot logged into Discord!");
            }
        }

        public async Task StopBotAsync()
        {
            if (_isRunning)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                _isRunning = false;
                Log.Information("Bot logged out of Discord!");
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

        private Task ReadyAsync()
        {
            Log.Information("Bot is connected and ready.");

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("BotService starting...");
            await StartBotAsync();
            await Task.Delay(-1, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("BotService stopping...");

            try
            {
                await StopBotAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during BotService cleanup.");
            }

            await base.StopAsync(cancellationToken);
            Log.Information("BotService stopped.");
        }
    }
}
