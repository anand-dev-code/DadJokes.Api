using System.Text.Json;
using DadJokes.Api.Data;
using DadJokes.Api.Models;
using Dapper;

namespace DadJokes.Api.Repositories
{
    public class JokeRepository : IJokeRepository
    {
        private readonly IDbConnectionFactory _dbFactory;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public JokeRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Joke?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            var sql = @"SELECT id, joke_text AS JokeText, source AS Source, word_count AS WordCount, created_at AS CreatedAt, external_id AS ExternalId
FROM jokes_app.jokes
WHERE id = @Id";
            return await conn.QuerySingleOrDefaultAsync<Joke>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Joke>> GetJokesByTermAsync(string term, int limit, CancellationToken cancellationToken = default)
        {
            await using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            var sql = @"
SELECT id, joke_text AS JokeText, source AS Source, word_count AS WordCount, created_at AS CreatedAt, external_id AS ExternalId
FROM jokes_app.jokes
WHERE joke_text ILIKE @Pattern
ORDER BY created_at DESC
LIMIT @Limit";
            return await conn.QueryAsync<Joke>(sql, new { Pattern = $"%{term}%", Limit = limit });
        }

        public async Task SaveJokesAsync(IEnumerable<Joke> jokes, string term, CancellationToken cancellationToken = default)
        {
            // Serialize jokes to JSON the stored proc expects, include externalId
            var jArray = new List<object>();
            foreach (var j in jokes)
            {
                jArray.Add(new
                {
                    id = j.Id != Guid.Empty ? j.Id : (Guid?)null,
                    externalId = string.IsNullOrWhiteSpace(j.ExternalId) ? null : j.ExternalId,
                    jokeText = j.JokeText,
                    source = j.Source,
                    wordCount = j.WordCount,
                    createdAt = j.CreatedAt == default ? (DateTimeOffset?)null : j.CreatedAt
                });
            }

            var json = JsonSerializer.Serialize(jArray, _jsonOptions);

            await using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            // Call schema-qualified stored function explicitly
            var sql = "SELECT jokes_app.sp_save_jokes(@Term, @Jokes::jsonb)";
            await conn.ExecuteAsync(sql, new { Term = term, Jokes = json });
        }

        public async Task<IEnumerable<Joke>> GetSavedJokesForTermAndGroupAsync(string term, JokeGroupType groupType, CancellationToken cancellationToken = default)
        {
            await using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            var sql = @"
SELECT j.id, j.joke_text AS JokeText, j.source AS Source, j.word_count AS WordCount, j.created_at AS CreatedAt, j.external_id AS ExternalId
FROM jokes_app.jokes j
JOIN jokes_app.saved_groups sg ON sg.joke_id = j.id
WHERE sg.term = @Term AND sg.group_type = @GroupType
ORDER BY sg.saved_at DESC
";
            return await conn.QueryAsync<Joke>(sql, new { Term = term, GroupType = groupType.ToString() });
        }

        public async Task<int> CountSavedJokesForTermAsync(string term, CancellationToken cancellationToken = default)
        {
            await using var conn = _dbFactory.CreateConnection();
            await conn.OpenAsync(cancellationToken);

            var sql = "SELECT COUNT(1) FROM jokes_app.saved_groups WHERE term = @Term";
            return await conn.ExecuteScalarAsync<int>(sql, new { Term = term });
        }
    }
}
