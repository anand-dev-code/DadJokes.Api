namespace DadJokes.Api.Models
{
    /// <summary>
    /// DTO for a single manual joke input.
    /// </summary>
    public class CustomJokeDto
    {
        public Guid Id { get; set; } = Guid.Empty;          // optional client-provided id
        public string? ExternalId { get; set; } = null;     // optional provider id (if available)
        public string? JokeText { get; set; } = null;       // required
        public string? Source { get; set; } = "Local";      // optional, default "Local"
        public DateTimeOffset? CreatedAt { get; set; } = null; // optional
    }
}
