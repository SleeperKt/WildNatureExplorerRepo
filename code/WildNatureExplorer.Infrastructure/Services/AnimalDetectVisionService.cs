using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WildNatureExplorer.Infrastructure.Services;

/// <summary>
/// Client for <see href="https://www.animaldetect.com/api/v1/detect">Animal Detect</see>.
/// </summary>
/// <remarks>
/// <para>
/// Working <c>curl</c> uses <c>multipart/form-data</c> with CRLF line endings and
/// plain <c>text/plain</c> field bodies for <c>country</c> / <c>threshold</c>.
/// Some upstream validators reject <see cref="MultipartFormDataContent"/> defaults
/// (boundary quoting, <c>charset=utf-8</c> on small text parts, StreamContent quirks).
/// We therefore build the wire payload explicitly — byte-identical in structure to
/// what curl generates — and POST it as a single <see cref="ByteArrayContent"/>.
/// </para>
/// </remarks>
public class AnimalDetectVisionService
{
    private const string DetectEndpoint = "https://www.animaldetect.com/api/v1/detect";
    private readonly HttpClient _http;
    private readonly string? _apiKey;

    public AnimalDetectVisionService(HttpClient http)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable("ANIMALDETECT_API_KEY")?.Trim();

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        _http.DefaultRequestHeaders.ExpectContinue = false;
    }

    public async Task<string> RecognizeAnimalAsync(byte[] imageBytes, string country = "USA", double threshold = 0.2)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "ANIMALDETECT_API_KEY is missing or empty. Set it in your environment / docker-compose .env file.");

        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image bytes are empty.", nameof(imageBytes));

        var thresholdStr = threshold.ToString("0.###", CultureInfo.InvariantCulture);
        var (mimeType, filename) = SniffImageMime(imageBytes);

        // 1) Manual multipart — mirrors curl -F wire format as closely as possible.
        using (var content = BuildManualMultipart(imageBytes, mimeType, filename, country, thresholdStr))
        {
            var label = await PostAndParseAsync(content);
            if (label != null) return label;
        }

        // 2) Same manual body but image-only (some deployments validate fewer fields).
        using (var contentImg = BuildManualMultipartImageOnly(imageBytes, mimeType, filename))
        {
            var label = await PostAndParseAsync(contentImg);
            if (label != null) return label;
        }

        // 3) Classic MultipartFormDataContent — last resort for exotic edge cases.
        using (var form = new MultipartFormDataContent())
        {
            var img = new ByteArrayContent(imageBytes);
            img.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            form.Add(img, "image", filename);

            var countryPart = new ByteArrayContent(Encoding.ASCII.GetBytes(country ?? ""));
            countryPart.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(countryPart, "country");

            var thrPart = new ByteArrayContent(Encoding.ASCII.GetBytes(thresholdStr));
            thrPart.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(thrPart, "threshold");

            var failed = await _http.PostAsync(DetectEndpoint, form);
            var errBody = await failed.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"AnimalDetect error ({failed.StatusCode}): {errBody}");
        }
    }

    private async Task<string?> PostAndParseAsync(HttpContent content)
    {
        var response = await _http.PostAsync(DetectEndpoint, content);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("annotations", out var annotations)
            || annotations.ValueKind != JsonValueKind.Array
            || annotations.GetArrayLength() == 0)
            return null;

        var first = annotations[0];
        if (!first.TryGetProperty("label", out var labelProp) || labelProp.ValueKind != JsonValueKind.String)
            return null;

        return labelProp.GetString() ?? "unknown animal";
    }

    /// <summary>multipart/form-data body for downstream HTTP expectations.</summary>
    private static ByteArrayContent BuildManualMultipart(
        byte[] imageBytes,
        string mimeType,
        string filename,
        string country,
        string thresholdStr)
    {
        var boundary = "----------------------------" + Guid.NewGuid().ToString("N");

        var safeFile = (filename ?? "upload.jpg").Replace("\\", "\\\\").Replace("\"", "\\\"");

        var sb = new StringBuilder(capacity: 512);
        sb.Append("--").Append(boundary).Append("\r\n");
        sb.Append("Content-Disposition: form-data; name=\"image\"; filename=\"").Append(safeFile).Append("\"\r\n");
        sb.Append("Content-Type: ").Append(mimeType).Append("\r\n");
        sb.Append("\r\n");

        var prefix = Encoding.UTF8.GetBytes(sb.ToString());

        var sb2 = new StringBuilder(capacity: 256);
        sb2.Append("\r\n--").Append(boundary).Append("\r\n");
        sb2.Append("Content-Disposition: form-data; name=\"country\"\r\n");
        sb2.Append("\r\n");
        sb2.Append(country ?? "");
        sb2.Append("\r\n--").Append(boundary).Append("\r\n");
        sb2.Append("Content-Disposition: form-data; name=\"threshold\"\r\n");
        sb2.Append("\r\n");
        sb2.Append(thresholdStr);
        sb2.Append("\r\n--").Append(boundary).Append("--\r\n");

        var suffix = Encoding.UTF8.GetBytes(sb2.ToString());

        var totalLen = prefix.Length + imageBytes.Length + suffix.Length;
        var buffer = new byte[totalLen];
        Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
        Buffer.BlockCopy(imageBytes, 0, buffer, prefix.Length, imageBytes.Length);
        Buffer.BlockCopy(suffix, 0, buffer, prefix.Length + imageBytes.Length, suffix.Length);

        var body = new ByteArrayContent(buffer);
        body.Headers.TryAddWithoutValidation(
            "Content-Type",
            $"multipart/form-data; boundary=\"{boundary}\"");
        return body;
    }

    private static ByteArrayContent BuildManualMultipartImageOnly(
        byte[] imageBytes,
        string mimeType,
        string filename)
    {
        var boundary = "----------------------------" + Guid.NewGuid().ToString("N");
        var safeFile = (filename ?? "upload.jpg").Replace("\\", "\\\\").Replace("\"", "\\\"");

        var sb = new StringBuilder(capacity: 512);
        sb.Append("--").Append(boundary).Append("\r\n");
        sb.Append("Content-Disposition: form-data; name=\"image\"; filename=\"").Append(safeFile).Append("\"\r\n");
        sb.Append("Content-Type: ").Append(mimeType).Append("\r\n");
        sb.Append("\r\n");

        var prefix = Encoding.UTF8.GetBytes(sb.ToString());
        var suffix = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

        var buffer = new byte[prefix.Length + imageBytes.Length + suffix.Length];
        Buffer.BlockCopy(prefix, 0, buffer, 0, prefix.Length);
        Buffer.BlockCopy(imageBytes, 0, buffer, prefix.Length, imageBytes.Length);
        Buffer.BlockCopy(suffix, 0, buffer, prefix.Length + imageBytes.Length, suffix.Length);

        var body = new ByteArrayContent(buffer);
        body.Headers.TryAddWithoutValidation(
            "Content-Type",
            $"multipart/form-data; boundary=\"{boundary}\"");
        return body;
    }

    private static (string MimeType, string Filename) SniffImageMime(ReadOnlySpan<byte> b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ("image/jpeg", "wildlife.jpg");

        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
            && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ("image/png", "wildlife.png");

        if (b.Length >= 6 && b[0] == (byte)'G' && b[1] == (byte)'I' && b[2] == (byte)'F'
            && b[3] == (byte)'8' && (b[4] == (byte)'7' || b[4] == (byte)'9') && b[5] == (byte)'a')
            return ("image/gif", "wildlife.gif");

        if (b.Length >= 12 && b[0] == (byte)'R' && b[1] == (byte)'I' && b[2] == (byte)'F' && b[3] == (byte)'F'
            && b[8] == (byte)'W' && b[9] == (byte)'E' && b[10] == (byte)'B' && b[11] == (byte)'P')
            return ("image/webp", "wildlife.webp");

        return ("application/octet-stream", "upload.bin");
    }
}
