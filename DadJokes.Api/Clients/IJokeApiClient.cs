namespace DadJokes.Api.Clients
{
    public interface IJokeApiClient
    {
        /// <summary>
        /// Fetch a random joke from icanhazdadjoke.
        /// </summary>
        Task<ExternalJokeDto?> GetRandomAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Search jokes by term. Limit up to 30 (icanhaz supports `limit` query param).
        /// </summary>
        Task<ExternalSearchResponseDto?> SearchAsync(string term, int limit = 30, CancellationToken cancellationToken = default);
    }
}
