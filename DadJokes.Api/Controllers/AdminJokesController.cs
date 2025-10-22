using DadJokes.Api.Models;
using DadJokes.Api.Repositories;
using DadJokes.Api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DadJokes.Api.Controllers
{
    [ApiController]
    [Route("api/admin/jokes")]
    public class AdminJokesController : ControllerBase
    {
        private readonly IJokeRepository _repo;

        public AdminJokesController(IJokeRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Add manual jokes and associate them with a term.
        /// The request body should contain a "term" and an array of "jokes".
        /// Each joke may include externalId (optional), jokeText, createdAt (optional).
        /// </summary>
        [HttpPost("custom")]
        public async Task<IActionResult> AddCustomJokes([FromBody] CustomJokesRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return BadRequest("Request body required.");
            if (string.IsNullOrWhiteSpace(request.Term)) return BadRequest("Term is required.");
            if (request.Jokes == null || !request.Jokes.Any()) return BadRequest("At least one joke is required.");

            var toSave = request.Jokes.Select(dto =>
            {
                var text = dto.JokeText ?? string.Empty;
                var wc = WordCounter.CountWords(text);
                return new Joke
                {
                    Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                    ExternalId = dto.ExternalId ?? string.Empty,
                    JokeText = text,
                    Source = string.IsNullOrWhiteSpace(dto.Source) ? "Local" : dto.Source,
                    WordCount = wc,
                    CreatedAt = dto.CreatedAt ?? DateTimeOffset.UtcNow
                };
            }).ToList();

            await _repo.SaveJokesAsync(toSave, request.Term, cancellationToken);

            // return saved count and created ids
            var createdIds = toSave.Select(j => j.Id).ToArray();
            return Created(string.Empty, new { term = request.Term, saved = createdIds.Length, ids = createdIds });
        }
    }
}
