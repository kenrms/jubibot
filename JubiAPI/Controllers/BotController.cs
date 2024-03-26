using DiscordBot;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(new
            {
                running = _botService.IsRunning(),
            });
        }

        [HttpGet("botconfig")]
        public IActionResult GetBotConfiguration()
        {
            return Ok(new
            {
                model = _botService.openAiModel,
                maxTokens = _botService.openAiMaxTokens,
                temperature = _botService.openAiTemp,
                systemPrompt = _botService.openAiSystemPrompt,
            });
        }

        [HttpPost("start")]
        public async ValueTask<IActionResult> StartBotAsync()
        {
            await _botService.StartBotAsync();

            return Ok();
        }

        [HttpPost("stop")]
        public async ValueTask<IActionResult> StopBotAsync()
        {
            await _botService.StopBotAsync();

            return Ok();
        }

        [HttpPost("temperature")]
        public IActionResult SetTemperature([FromBody] float temperature)
        {
            _botService.SetTemperature(temperature);

            return Ok();
        }

        [HttpPost("maxtokens")]
        public IActionResult SetMaxTokens([FromBody] int maxTokens)
        {
            _botService.SetOpenAiMaxTokens(maxTokens);

            return Ok();
        }

        [HttpPost("systemprompt")]
        public IActionResult SetSystemPrompt([FromBody] string prompt)
        {
            _botService.SetOpenAiSystemPrompt(prompt);

            return Ok();
        }

        [HttpPost("model")]
        public IActionResult SetModel([FromBody] string model)
        {
            _botService.SetOpenAiModel(model);

            return Ok();
        }
    }
}
