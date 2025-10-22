using DadJokes.Api.Models;

namespace DadJokes.Api.Repositories
{
    public interface IJokeRepository
    {
        Task<Joke?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Joke>> GetJokesByTermAsync(string term, int limit, CancellationToken cancellationToken = default);
        Task SaveJokesAsync(IEnumerable<Joke> jokes, string term, CancellationToken cancellationToken = default);
        Task<IEnumerable<Joke>> GetSavedJokesForTermAndGroupAsync(string term, JokeGroupType groupType, CancellationToken cancellationToken = default);
        Task<int> CountSavedJokesForTermAsync(string term, CancellationToken cancellationToken = default);
    }
}
