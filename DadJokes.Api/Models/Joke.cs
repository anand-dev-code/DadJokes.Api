namespace DadJokes.Api.Models
{
    public class Joke
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string JokeText { get; set; } = string.Empty;
        public string Source { get; set; } = "External"; // External or Local
        public int WordCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
