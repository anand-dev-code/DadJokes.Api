using DadJokes.Api.Models;
using DadJokes.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DadJokes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JokesController : ControllerBase
    {
        private readonly IJokeService _service;
        private readonly ILogger<JokesController> _logger;

        public JokesController(IJokeService service, ILogger<JokesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandom(CancellationToken cancellationToken)
        {
            var joke = await _service.GetRandomJokeAsync(cancellationToken);
            if (joke == null) return StatusCode(502, "Failed to fetch from external API");
            return Ok(joke);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("term is required");

            var grouped = await _service.SearchJokesAsync(term, cancellationToken);
            return Ok(grouped);
        }
    }
}
