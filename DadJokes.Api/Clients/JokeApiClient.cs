using System.Web;

namespace DadJokes.Api.Clients
{
    public class JokeApiClient : IJokeApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<JokeApiClient> _logger;

        public JokeApiClient(HttpClient http, ILogger<JokeApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<ExternalJokeDto?> GetRandomAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var resp = await _http.GetFromJsonAsync<ExternalJokeDto>("/", cancellationToken);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching random joke from external API");
                return null;
            }
        }

        public async Task<ExternalSearchResponseDto?> SearchAsync(string term, int limit = 30, CancellationToken cancellationToken = default)
        {
            try
            {
                var qb = HttpUtility.ParseQueryString(string.Empty);
                qb["term"] = term;
                qb["limit"] = limit.ToString();
                var url = "/search?" + qb.ToString();

                var resp = await _http.GetFromJsonAsync<ExternalSearchResponseDto?>(url, cancellationToken);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jokes from external API for term {Term}", term);
                return null;
            }
        }
    }
}
