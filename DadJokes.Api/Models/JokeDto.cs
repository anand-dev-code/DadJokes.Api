namespace DadJokes.Api.Models
{
    public class JokeDto
    {
        public Guid Id { get; set; }
        public string JokeText { get; set; } = string.Empty;
        public JokeGroupType GroupType { get; set; }
        public int WordCount { get; set; }
        public string Source { get; set; } = "External";
    }
}
