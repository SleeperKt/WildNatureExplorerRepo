using System.Net.Http.Headers;
using System.Text.Json;

namespace WildNatureExplorer.Infrastructure.Services;

public class HuggingFaceVisionService
{
    private readonly HttpClient _http;

    public HuggingFaceVisionService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                Environment.GetEnvironmentVariable("HF_API_KEY")
            );
    }

    public async Task<string> RecognizeAnimalAsync(byte[] imageBytes)
    {
        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType =
            new MediaTypeHeaderValue("image/jpeg");

        var response = await _http.PostAsync(
            "https://router.huggingface.co/hf-inference/models/microsoft/resnet-18",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"HuggingFace error ({response.StatusCode}): {error}"
            );
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement[0].GetProperty("label").GetString()!;
    }
}
