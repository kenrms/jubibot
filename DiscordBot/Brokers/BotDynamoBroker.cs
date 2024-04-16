using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DiscordBot.Brokers
{
    public class BotDynamoBroker : IBotBroker
    {
        private AmazonDynamoDBClient _dynamoClient;
        private const string tableName = "BotConfigurations";
        private const int botId = 1;    // single bot for now

        public BotDynamoBroker()
        {
            _dynamoClient = new AmazonDynamoDBClient();
            InitializeTableAsync();
        }

        public async Task<BotConfiguration> GetBotConfiguration()
        {
            return await GetBotConfigurationAsync(botId);
        }

        public async Task UpdateOpenAiMaxTokens(int newMaxTokens)
        {
            await SetBotConfigurationAsync(botId, "OpenAiMaxTokens", newMaxTokens.ToString());
        }

        public async Task UpdateOpenAiModel(string newModel)
        {
            await SetBotConfigurationAsync(botId, "OpenAiModel", newModel);
        }

        public async Task UpdateOpenAiSystemPrompt(string newSystemPrompt)
        {
            await SetBotConfigurationAsync(botId, "OpenAiSystemPrompt", newSystemPrompt);
        }

        public async Task UpdateOpenAiTemperature(float newTemperature)
        {
            await SetBotConfigurationAsync(botId, "OpenAiTemperature", newTemperature.ToString());
        }

        private async Task InitializeTableAsync()
        {
            ListTablesResponse tables = await _dynamoClient.ListTablesAsync();

            if (!tables.TableNames.Contains(tableName))
            {
                await CreateTableAsync();
            }
        }

        private async Task CreateTableAsync()
        {
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Id", ScalarAttributeType.N),
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Id", KeyType.HASH),
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                }
            };

            await _dynamoClient.CreateTableAsync(request);
        }

        private async Task SetBotConfigurationAsync(int botId, string configurationKey, string configurationValue)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = new Dictionary<string, AttributeValue>
            {
                {"Id", new AttributeValue {N = botId.ToString()}},
                {"ConfigurationKey", new AttributeValue {S = configurationKey}},
                {"ConfigurationValue", new AttributeValue {S = configurationValue}}
            }
            };

            var response = await _dynamoClient.PutItemAsync(request);
            // TODO error handling, etc.
        }

        private async Task<BotConfiguration> GetBotConfigurationAsync(int botId)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Id", new AttributeValue {N = botId.ToString()}},
                    {"ConfigurationKey", new AttributeValue {S = "OpenAiConfiguration"}}
                }
            };

            var response = await _dynamoClient.GetItemAsync(request);

            if (response.Item == null || !response.Item.ContainsKey("ConfigurationValue"))
            {
                return null;
            }

            return new BotConfiguration
            {
                OpenAiMaxTokens = int.Parse(response.Item["OpenAiMaxTokens"].S),
                OpenAiModel = response.Item["OpenAiModel"].S,
                OpenAiSystemPrompt = response.Item["OpenAiSystemPrompt"].S,
                OpenAiTemperature = float.Parse(response.Item["OpenAiTemperature"].S)
            };
        }
    }
}
