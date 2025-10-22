namespace DadJokes.Api.Models
{
    /// <summary>
    /// DTO for a single manual joke input.
    /// </summary>
    /// <summary>
    /// Request payload to add manual jokes for a term.
    /// </summary>
    public class CustomJokesRequestDto
    {
        public string Term { get; set; } = string.Empty;
        public List<CustomJokeDto>? Jokes { get; set; }
    }
}
