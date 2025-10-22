using System.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Microsoft.OpenApi.Models;
using DadJokes.Api.Clients;
using DadJokes.Api.Data;
using DadJokes.Api.Repositories;
using DadJokes.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jokes API", Version = "v1" });
});

// Configuration
var icanhazBase = builder.Configuration.GetValue<string>("Icanhaz:BaseUrl") ?? "https://icanhazdadjoke.com/";

// HttpClient for icanhazdadjoke
builder.Services.AddHttpClient<IJokeApiClient, JokeApiClient>(client =>
{
    client.BaseAddress = new Uri(icanhazBase);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("JokesApi/1.0 (anand-dev-code)");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetTimeoutPolicy());

// Database connection factory
builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty));

// DI for repositories and services
builder.Services.AddScoped<IJokeRepository, JokeRepository>();
builder.Services.AddScoped<IJokeService, JokeService>();

// Memory cache for short-term caching of searches
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4)
        });
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}