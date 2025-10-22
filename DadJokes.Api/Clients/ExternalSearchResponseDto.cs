using System.Text.Json.Serialization;

namespace DadJokes.Api.Clients
{
    public record ExternalSearchResponseDto(
     [property: JsonPropertyName("current_page")] int CurrentPage,
     [property: JsonPropertyName("limit")] int Limit,
     [property: JsonPropertyName("next_page")] int? NextPage,
     [property: JsonPropertyName("previous_page")] int? PreviousPage,
     [property: JsonPropertyName("results")] ExternalJokeDto[] Results,
     [property: JsonPropertyName("search_term")] string SearchTerm,
     [property: JsonPropertyName("status")] int Status,
     [property: JsonPropertyName("total_jokes")] int TotalJokes,
     [property: JsonPropertyName("total_pages")] int TotalPages
 );
}
