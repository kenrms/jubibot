using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace JubiAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;

        public BotController(IBotService botService)
        {
            _botService = botService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            Log.Information("Retrieving Bot Status...");

            return Ok(new
            {
                running = _botService.IsRunning(),
            });
        }

        [HttpGet("botconfig")]
        public IActionResult GetBotConfiguration()
        {
            Log.Information("Retrieving Bot Configuration...");

            return Ok(new BotConfiguration()
            {
                OpenAiMaxTokens = _botService.openAiMaxTokens,
                OpenAiModel = _botService.openAiModel,
                OpenAiSystemPrompt = _botService.openAiSystemPrompt,
                OpenAiTemperature = _botService.openAiTemp,
            });
        }

        [HttpPost("start")]
        public async ValueTask<IActionResult> StartBotAsync()
        {
            Log.Information("Starting Discord Bot");
            await _botService.StartBotAsync();

            return Ok();
        }

        [HttpPost("stop")]
        public async ValueTask<IActionResult> StopBotAsync()
        {
            Log.Information("Stopping Discord Bot");
            await _botService.StopBotAsync();

            return Ok();
        }

        [HttpPost("temperature")]
        public IActionResult SetTemperature([FromBody] float temperature)
        {
            Log.Information("Setting bot temperature to {temperature}", temperature);
            _botService.SetTemperature(temperature);

            return Ok();
        }

        [HttpPost("maxtokens")]
        public IActionResult SetMaxTokens([FromBody] int maxTokens)
        {
            Log.Information("Setting bot maxtokens to {maxTokens}", maxTokens);
            _botService.SetOpenAiMaxTokens(maxTokens);

            return Ok();
        }

        [HttpPost("systemprompt")]
        public IActionResult SetSystemPrompt([FromBody] string prompt)
        {
            Log.Information("Updating System prompt");
            _botService.SetOpenAiSystemPrompt(prompt);

            return Ok();
        }

        [HttpPost("model")]
        public IActionResult SetModel([FromBody] string model)
        {
            Log.Information("Setting openai model to {model}", model);
            _botService.SetOpenAiModel(model);

            return Ok();
        }
    }
}
