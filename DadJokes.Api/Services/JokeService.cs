using DadJokes.Api.Clients;
using DadJokes.Api.Models;
using DadJokes.Api.Repositories;
using DadJokes.Api.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace DadJokes.Api.Services
{
    public class JokeService : IJokeService
    {
        private readonly IJokeApiClient _external;
        private readonly IJokeRepository _repo;
        private readonly ILogger<JokeService> _logger;
        private readonly IMemoryCache _cache;

        private const int MaxResults = 30;

        public JokeService(IJokeApiClient external, IJokeRepository repo, ILogger<JokeService> logger, IMemoryCache cache)
        {
            _external = external;
            _repo = repo;
            _logger = logger;
            _cache = cache;
        }

        public async Task<JokeDto?> GetRandomJokeAsync(CancellationToken cancellationToken = default)
        {
            var ext = await _external.GetRandomAsync(cancellationToken);
            if (ext == null) return null;

            var wc = WordCounter.CountWords(ext.Joke);
            var model = new JokeDto
            {
                Id = Guid.NewGuid(),
                JokeText = ext.Joke,
                WordCount = wc,
                GroupType = GroupFromCount(wc),
                Source = "External"
            };

            return model;
        }

        public async Task<GroupedJokesDto> SearchJokesAsync(string term, CancellationToken cancellationToken = default)
        {
            term = term?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(term))
                return new GroupedJokesDto { Term = term };

            // 1) Try to return saved jokes first
            var savedCount = await _repo.CountSavedJokesForTermAsync(term, cancellationToken);
            var shortSaved = (await _repo.GetSavedJokesForTermAndGroupAsync(term, JokeGroupType.Short, cancellationToken)).ToList();
            var mediumSaved = (await _repo.GetSavedJokesForTermAndGroupAsync(term, JokeGroupType.Medium, cancellationToken)).ToList();
            var longSaved = (await _repo.GetSavedJokesForTermAndGroupAsync(term, JokeGroupType.Long, cancellationToken)).ToList();

            // If saved >= MaxResults, return saved prioritized
            var grouped = new GroupedJokesDto { Term = term };

            void MapSaved(IEnumerable<Joke> list, List<JokeDto> target)
            {
                foreach (var j in list)
                {
                    var highlighted = TextHighlighter.Highlight(j.JokeText, term);
                    target.Add(new JokeDto
                    {
                        Id = j.Id,
                        JokeText = highlighted,
                        WordCount = j.WordCount,
                        GroupType = GroupFromCount(j.WordCount),
                        Source = j.Source
                    });
                }
            }

            MapSaved(shortSaved, grouped.Short);
            MapSaved(mediumSaved, grouped.Medium);
            MapSaved(longSaved, grouped.Long);

            var totalSoFar = grouped.Short.Count + grouped.Medium.Count + grouped.Long.Count;

            if (totalSoFar >= MaxResults)
            {
                // Trim to MaxResults preserving group order
                TrimToLimit(grouped, MaxResults);
                return grouped;
            }

            // 2) Fetch external API for remaining results
            var remaining = MaxResults - totalSoFar;
            var externalResp = await _external.SearchAsync(term, remaining, cancellationToken);
            if (externalResp?.Results != null && externalResp.Results.Any())
            {
                var toSave = new List<Joke>();
                foreach (var ext in externalResp.Results)
                {
                    var wc = WordCounter.CountWords(ext.Joke);
                    var joke = new Joke
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = ext.Id,
                        JokeText = ext.Joke,
                        Source = "External",
                        WordCount = wc,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    var highlighted = TextHighlighter.Highlight(joke.JokeText, term);
                    var dto = new JokeDto
                    {
                        Id = joke.Id,
                        JokeText = highlighted,
                        WordCount = wc,
                        GroupType = GroupFromCount(wc),
                        Source = joke.Source
                    };

                    switch (dto.GroupType)
                    {
                        case JokeGroupType.Short:
                            if (grouped.Short.Count < MaxResults) grouped.Short.Add(dto);
                            break;
                        case JokeGroupType.Medium:
                            if (grouped.Medium.Count < MaxResults) grouped.Medium.Add(dto);
                            break;
                        case JokeGroupType.Long:
                            if (grouped.Long.Count < MaxResults) grouped.Long.Add(dto);
                            break;
                    }

                    toSave.Add(joke);
                }

                // Persist fetched jokes and group associations
                if (toSave.Any())
                {
                    try
                    {
                        await _repo.SaveJokesAsync(toSave, term, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to persist fetched jokes for term {Term}", term);
                    }
                }
            }

            // final trimming
            TrimToLimit(grouped, MaxResults);

            return grouped;
        }

        private static JokeGroupType GroupFromCount(int wc)
        {
            if (wc < 10) return JokeGroupType.Short;
            if (wc < 20) return JokeGroupType.Medium;
            return JokeGroupType.Long;
        }

        private static void TrimToLimit(GroupedJokesDto grouped, int limit)
        {
            // Trim each group proportionally if overall > limit.
            var all = grouped.Short.Count + grouped.Medium.Count + grouped.Long.Count;
            if (all <= limit) return;

            // Simpler approach: flatten then take top N by group preference: Short, Medium, Long
            var flattened = new List<JokeDto>();
            flattened.AddRange(grouped.Short);
            flattened.AddRange(grouped.Medium);
            flattened.AddRange(grouped.Long);

            var take = flattened.Take(limit).ToList();

            grouped.Short = take.Where(j => j.GroupType == JokeGroupType.Short).ToList();
            grouped.Medium = take.Where(j => j.GroupType == JokeGroupType.Medium).ToList();
            grouped.Long = take.Where(j => j.GroupType == JokeGroupType.Long).ToList();
        }
    }
}
