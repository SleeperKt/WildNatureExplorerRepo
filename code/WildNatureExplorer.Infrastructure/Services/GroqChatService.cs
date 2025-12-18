using System.Net.Http.Headers;
using System.Text.Json;
using WildNatureExplorer.Application.AI.PromptPolicies;
using System.Net.Http.Json;

namespace WildNatureExplorer.Infrastructure.Services;

public class GroqChatService
{
    private readonly HttpClient _http;

    public GroqChatService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
                Environment.GetEnvironmentVariable("GROQ_API_KEY"));
    }

    public async Task<string> AskAsync(string userPrompt)
    {
        AnimalPromptPolicy.Validate(userPrompt);

        var body = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "system", content = AnimalPromptPolicy.BuildSystemPrompt() },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await _http.PostAsJsonAsync("chat/completions", body);
        
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json); // TEMP: check the actual response
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("completion", out var completionElement))
        {
            return completionElement.GetString()!;
        }
        else if (doc.RootElement.TryGetProperty("text", out var textElement))
        {
            return textElement.GetString()!;
        }
        else
        {
            // fallback / error logging
            throw new InvalidOperationException($"Unexpected Groq response: {json}");
        }

        // return doc.RootElement
        //           .GetProperty("choices")[0]
        //           .GetProperty("message")
        //           .GetProperty("content")
        //           .GetString()!;
    }
}

