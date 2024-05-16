using AutoBogus;
using Bogus;
using Discord;
using DiscordBot.Brokers;
using DiscordBot.Models;
using DiscordBot.Services;
using FluentAssertions;
using Moq;

namespace DiscordBot.Tests.Services
{
    public partial class BotServiceTests
    {
        private readonly IBotService botService;
        private Mock<ICustomConfiguration> configurationMock;
        private Mock<IBotBroker> botBrokerMock;
        private Mock<IDiscordClientBroker> discordClientBrokerMock;

        private string _discordToken;
        private string _openAiKey;
        private BotConfiguration _botConfiguration;

        public BotServiceTests()
        {
            _discordToken = GetRandomString();
            _openAiKey = GetRandomString();
            _botConfiguration = GetRandomBotConfiguration();

            configurationMock = new Mock<ICustomConfiguration>();
            botBrokerMock = new Mock<IBotBroker>();
            discordClientBrokerMock = new Mock<IDiscordClientBroker>();

            botBrokerMock.Setup(b => b.GetBotConfiguration()).ReturnsAsync(_botConfiguration);
            configurationMock.Setup(config => config.GetDiscordToken()).Returns(_discordToken);
            configurationMock.Setup(config => config.GetOpenAiKey()).Returns(_openAiKey);

            botService = new BotService(
                config: configurationMock.Object,
                botBroker: botBrokerMock.Object,
                discordClientBroker: discordClientBrokerMock.Object);
        }

        public static string GetRandomString() => new Faker().Random.Word();

        public static BotConfiguration GetRandomBotConfiguration() =>
            new AutoFaker<BotConfiguration>().Generate();

        [Fact]
        public async void ShouldStartAsync()
        {
            // given
            TokenType inputTokenType = TokenType.Bot;
            string inputDiscordToken = _discordToken;

            discordClientBrokerMock
                .Setup(broker =>
                    broker.LoginAsync(It.IsAny<TokenType>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            discordClientBrokerMock
                .Setup(broker => broker.StartAsync())
                .Returns(Task.CompletedTask);

            // when
            await botService.StartBotAsync();

            // then
            botService.IsRunning().Should().BeTrue();

            discordClientBrokerMock.Verify(broker =>
                broker.LoginAsync(inputTokenType, inputDiscordToken, true),
                Times.Once);

            discordClientBrokerMock.Verify(broker =>
                broker.StartAsync(),
                Times.Once);
        }
    }
}
