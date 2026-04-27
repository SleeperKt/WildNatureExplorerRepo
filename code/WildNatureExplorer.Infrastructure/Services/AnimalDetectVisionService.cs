using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WildNatureExplorer.Infrastructure.Services;

public class AnimalDetectVisionService
{
    private const string DetectEndpoint = "https://www.animaldetect.com/api/v1/detect";
    private readonly HttpClient _http;

    public AnimalDetectVisionService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                Environment.GetEnvironmentVariable("ANIMALDETECT_API_KEY")
            );
    }

    public async Task<string> RecognizeAnimalAsync(byte[] imageBytes, string country = "USA", double threshold = 0.2)
    {
        using var form = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        form.Add(imageContent, "image", "wildlife.jpg");
        form.Add(new StringContent(country), "country");
        form.Add(new StringContent(threshold.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)), "threshold");

        var response = await _http.PostAsync(DetectEndpoint, form);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"AnimalDetect error ({response.StatusCode}): {error}"
            );
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("annotations", out var annotations)
            || annotations.ValueKind != JsonValueKind.Array
            || annotations.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("AnimalDetect returned no annotations.");
        }

        var first = annotations[0];
        if (!first.TryGetProperty("label", out var labelProp) || labelProp.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException("AnimalDetect response did not include a valid label.");
        }

        return labelProp.GetString() ?? "unknown animal";
    }
}
