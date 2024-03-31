using DiscordBot;
using DiscordBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace JubiAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly ILogger<BotController> _logger;
        private readonly IBotService _botService;

        public BotController(ILogger<BotController> logger, IBotService botService)
        {
            _logger = logger;
            _botService = botService;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            _logger.LogInformation("Retrieving Bot Status...");

            return Ok(new
            {
                running = _botService.IsRunning(),
            });
        }

        [HttpGet("botconfig")]
        public IActionResult GetBotConfiguration()
        {
            _logger.LogInformation("Retrieving Bot Configuration...");

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
            _logger.LogInformation("Starting Discord Bot");
            await _botService.StartBotAsync();

            return Ok();
        }

        [HttpPost("stop")]
        public async ValueTask<IActionResult> StopBotAsync()
        {
            _logger.LogInformation("Stopping Discord Bot");
            await _botService.StopBotAsync();

            return Ok();
        }

        [HttpPost("temperature")]
        public IActionResult SetTemperature([FromBody] float temperature)
        {
            _logger.LogInformation("Setting bot temperature to {temperature}", temperature);
            _botService.SetTemperature(temperature);

            return Ok();
        }

        [HttpPost("maxtokens")]
        public IActionResult SetMaxTokens([FromBody] int maxTokens)
        {
            _logger.LogInformation("Setting bot maxtokens to {maxTokens}", maxTokens);
            _botService.SetOpenAiMaxTokens(maxTokens);

            return Ok();
        }

        [HttpPost("systemprompt")]
        public IActionResult SetSystemPrompt([FromBody] string prompt)
        {
            _logger.LogInformation("Updating System prompt");
            _botService.SetOpenAiSystemPrompt(prompt);

            return Ok();
        }

        [HttpPost("model")]
        public IActionResult SetModel([FromBody] string model)
        {
            _logger.LogInformation("Setting openai model to {model}", model);
            _botService.SetOpenAiModel(model);

            return Ok();
        }
    }
}
