using System.Text.Json.Serialization;

namespace DadJokes.Api.Clients
{
    public record ExternalJokeDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("joke")] string Joke,
    [property: JsonPropertyName("status")] int Status
);
}
