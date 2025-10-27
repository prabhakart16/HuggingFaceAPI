using API.models.HuggingFaceApiDemo.Models;
using API.models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

// Add a named CORS policy to allow the Angular dev server
var angularOrigin = "http://localhost:4200";
var corsPolicyName = "AllowAngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(angularOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Capture IConfiguration so it can be used like a readonly field in this file
var _configuration = builder.Configuration;

var app = builder.Build();

// Enable CORS before mapping endpoints
app.UseCors(corsPolicyName);

app.MapPost("api/ask", async (HuggingFaceRequest request, IHttpClientFactory factory) =>
{
    //if (string.IsNullOrWhiteSpace(request.Model) || string.IsNullOrWhiteSpace(request.Prompt))
    //    return Results.BadRequest(new { error = "Both 'model' and 'prompt' are required." });

    // Prefer configuration value, fall back to environment variable
    var hfToken = _configuration["HF_TOKEN"] ?? Environment.GetEnvironmentVariable("HF_TOKEN");
    if (string.IsNullOrWhiteSpace(hfToken))
        return Results.Problem("Missing HF_TOKEN configuration or environment variable", statusCode: 500);

    var http = factory.CreateClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfToken);

    //var body = new
    //{
    //    model = request.Model,
    //    prompt = request.Prompt
    //};
       var body = new ChatRequest
    {
        Model = "openai/gpt-oss-20b:groq",
        Stream = false,
        Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = request.Prompt }
            }
    };
    var jsonbody = JsonSerializer.Serialize(body, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    var content = new StringContent(jsonbody, Encoding.UTF8, "application/json");

    var response = await http.PostAsync("https://router.huggingface.co/v1/chat/completions", content);
    var json = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        return Results.Problem($"Hugging Face API error: {response.StatusCode}\n{json}", statusCode: (int)response.StatusCode);

    return Results.Content(json, "application/json");
});
app.MapGet("api/models", () =>
{
    var models = new[]
    {
        new { id = "google/gemma-2-9b", name = "Google Gemma 2 9B Instruct", provider = "Hugging Face" },
        new { id = "deepset/roberta-base-squad2", name = "RoBERTa Base SQuAD2", provider = "Hugging Face" },
        new { id = "gpt2", name = "GPT-2", provider = "Hugging Face" },
        new { id = "mistralai/Mistral-7B-Instruct-v0.2", name = "Mistral 7B Instruct", provider = "Hugging Face" }
    };

    return Results.Ok(new { models });
});
app.Run();
