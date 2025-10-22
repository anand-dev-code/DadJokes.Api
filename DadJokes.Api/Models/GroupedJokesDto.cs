namespace DadJokes.Api.Models
{

    public class GroupedJokesDto
    {
        public string Term { get; set; } = string.Empty;
        public List<JokeDto> Short { get; set; } = new();
        public List<JokeDto> Medium { get; set; } = new();
        public List<JokeDto> Long { get; set; } = new();
    }
}
