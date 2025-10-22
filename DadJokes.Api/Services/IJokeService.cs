using DadJokes.Api.Models;

namespace DadJokes.Api.Services
{
    public interface IJokeService
    {
        Task<JokeDto?> GetRandomJokeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Search by term. Returns at most 30 jokes grouped. If DB has saved jokes they will be prioritized.
        /// </summary>
        Task<GroupedJokesDto> SearchJokesAsync(string term, CancellationToken cancellationToken = default);
    }
}
