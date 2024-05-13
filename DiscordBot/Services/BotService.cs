using Azure;
using Azure.AI.OpenAI;
using Discord;
using Discord.WebSocket;
using DiscordBot.Brokers;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiscordBot.Services
{
    public class BotService : BackgroundService, IBotService
    {
        private readonly string discordToken;
        private readonly string openAiKey;
        private readonly IBotBroker _botBroker;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private bool _isRunning;
        private const int maxConversationHistory = 100;
        private readonly string _botName = Guid.NewGuid().ToString();

        private Dictionary<ulong, Queue<DiscordMessage>> _conversationHistory;

        public float openAiTemp { get; private set; }
        public int openAiMaxTokens { get; private set; }
        public string openAiSystemPrompt { get; private set; }
        public string openAiModel { get; private set; }

        public bool IsRunning() => _isRunning;
        public async Task<float> GetTemperatureAsync() => (await _botBroker.GetBotConfiguration()).OpenAiTemperature;

        public BotService(IConfiguration config, IBotBroker botBroker)
        {
            _botBroker = botBroker;
            _config = config;
            InitializeConfigurationAsync().Wait();

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
            _conversationHistory = new Dictionary<ulong, Queue<DiscordMessage>>();
            InitializeClient();
        }

        private void InitializeClient()
        {
            _client.Log += HandleDiscordClientLogging;
            _client.MessageReceived += MessageHandler;
            _client.Ready += ReadyAsync;
            _isRunning = false;
        }

        private async Task InitializeConfigurationAsync()
        {
            BotConfiguration botConfig = await _botBroker.GetBotConfiguration();
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

            var channelId = message.Channel.Id;
            var messageContent = message.Content;
            var userName = message.Author.Username;
            var timestamp = message.Timestamp;

            Log.Information($"Received message in {channelId} from {userName}: {messageContent}");

            if (!_conversationHistory.ContainsKey(channelId))
            {
                _conversationHistory[channelId] = new Queue<DiscordMessage>();
            }

            var channelConversation = _conversationHistory[channelId];

            if (channelConversation.Count >= maxConversationHistory)
            {
                channelConversation.Dequeue();
            }

            channelConversation.Enqueue(new DiscordMessage(message));
            string response = await GetOpenAiResponse(channelConversation);
            Log.Information($"Response from bot in {channelId}: {response}");

            // include bot response for now also
            channelConversation.Enqueue(new DiscordMessage
            {
                Timestamp = DateTime.UtcNow,
                Username = _botName,
                Message = response,
            });

            await ReplyAsync(message, response);
        }

        private async Task<string> GetOpenAiResponse(Queue<DiscordMessage> channelConversation)
        {
            if (string.IsNullOrEmpty(openAiKey))
            {
                throw new Exception($"Invalid OpenAI key provided: {openAiKey}");
            }

            var client = new OpenAIClient(openAiKey);

            var messages = new List<ChatRequestMessage> { new ChatRequestSystemMessage(openAiSystemPrompt), };

            foreach (var message in channelConversation)
            {
                if (message.Username == _botName)
                {
                    messages.Add(new ChatRequestAssistantMessage($"{message.Message}"));
                }
                else
                {
                    string body = string.Empty;

                    if (!string.IsNullOrEmpty(message.ReferenceMessage))
                    {
                        body = $"This message is in response to another message. Here is the response: \n\n --- \n\n " +
                            $"Author: {message.Username}\n" +
                            $"Message: {message.Message}\n\n" +
                            $"---\n\n" +
                            $"This is the message this this is responding to:\n\n" +
                            $"---\n\n" +
                            $"Reference Author: {message.ReferenceAuthor}\n" +
                            $"Refereance Message: {message.ReferenceMessage}";
                    }
                    else
                    {
                        body = $"Author: {message.Username} \nMessage: {message.Message}";
                    }

                    messages.Add(new ChatRequestUserMessage(body));
                }
            }

            var chatCompletionsOptions = new ChatCompletionsOptions(openAiModel, messages)
            {
                Temperature = openAiTemp,
                MaxTokens = openAiMaxTokens
            };

            try
            {
                Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
                ChatResponseMessage responseMessage = response.Value.Choices[0].Message;

                return responseMessage.Content;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting OpenAI response");

                return "Sorry, there was an error getting a response from OpenAI. Please try again later.";
            }
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
            _botBroker.UpdateOpenAiTemperature(newTemp);
        }

        public void SetOpenAiMaxTokens(int maxTokens)
        {
            openAiMaxTokens = maxTokens;
            _botBroker.UpdateOpenAiMaxTokens(maxTokens);
        }

        public void SetOpenAiModel(string model)
        {
            openAiModel = model;
            _botBroker.UpdateOpenAiModel(model);
        }

        public void SetOpenAiSystemPrompt(string systemPrompt)
        {
            openAiSystemPrompt = systemPrompt;
            _botBroker.UpdateOpenAiSystemPrompt(systemPrompt);
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
