using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;


namespace DiscordBot.Brokers
{
    public class DynamoDBBotBroker : IBotBroker
    {
        private IAmazonDynamoDB _client;
        private readonly IConfiguration _config;
        private readonly string tableName = "BotConfigurations";
        private const int botId = 1;    // single bot for now

        public DynamoDBBotBroker(IConfiguration config)
        {
            _config = config;
        }

        private IAmazonDynamoDB GetClient()
        {
            bool isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);
            string accessKey, secret;

            if (isDevelopment)
            {
                accessKey = _config["AWSAccessKey"];
                secret = _config["AWSSecret"];
            }
            else
            {
                accessKey = Environment.GetEnvironmentVariable("AWSAccessKey");
                secret = Environment.GetEnvironmentVariable("AWSSecret");
            }

            var credentials = new BasicAWSCredentials(accessKey, secret);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);

            return client;
        }

        public async Task<BotConfiguration> GetBotConfiguration()
        {
            var getRequest = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Id", new AttributeValue { N = botId.ToString() } },
                }
            };

            try
            {
                var client = GetClient();
                var response = await client.GetItemAsync(getRequest);

                if (!response.Item.Any())
                {
                    // Handle case where configuration is not found (implement default logic if needed)
                    return null;
                }

                return ConvertFromDynamoDb(response.Item);
            }
            catch (Exception ex)
            {
                // Implement error handling (e.g., logging)
                Console.WriteLine($"Error getting item: {ex.Message}");
                throw; // Or rethrow with a custom exception type
            }
        }

        public async Task UpdateOpenAiTemperature(float newTemperature)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                  { "Id", new AttributeValue { S = "config" } }
                },
                UpdateExpression = "SET OpenAiTemperature = :t",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                  { ":t", new AttributeValue { N = newTemperature.ToString() } }
                }
            };

            var response = await _client.UpdateItemAsync(updateRequest);
            // TODO handle errors
        }

        public async Task UpdateOpenAiMaxTokens(int newMaxTokens)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                  { "Id", new AttributeValue { S = "config" } }
                },
                UpdateExpression = "SET OpenAiMaxTokens = :t",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                  { ":t", new AttributeValue { N = newMaxTokens.ToString() } }
                }
            };

            var response = await _client.UpdateItemAsync(updateRequest);
            // TODO handle errors
        }

        public async Task UpdateOpenAiModel(string newModel)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                  { "Id", new AttributeValue { S = "config" } }
                },
                UpdateExpression = "SET OpenAiModel = :m",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                  { ":m", new AttributeValue { S = newModel } }
                }
            };

            var response = _client.UpdateItemAsync(updateRequest);
            // TODO handle errors
        }

        public async Task UpdateOpenAiSystemPrompt(string newSystemPrompt)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                  { "Id", new AttributeValue { S = "config" } }
                },
                UpdateExpression = "SET OpenAiSystemPrompt = :p",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                  { ":p", new AttributeValue { S = newSystemPrompt } }
                }
            };

            var response = _client.UpdateItemAsync(updateRequest);

            // TODO handle errors
        }

        private BotConfiguration ConvertFromDynamoDb(Dictionary<string, AttributeValue> attributes)
        {
            return new BotConfiguration
            {
                OpenAiTemperature = float.Parse(attributes["OpenAiTemperature"].N),
                OpenAiMaxTokens = int.Parse(attributes["OpenAiMaxTokens"].N),
                OpenAiSystemPrompt = attributes["OpenAiSystemPrompt"].S,
                OpenAiModel = attributes["OpenAiModel"].S
            };
        }
    }
}
