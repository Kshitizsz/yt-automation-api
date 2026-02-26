using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YTAutomation.Core.Interfaces;

namespace YTAutomation.Infrastructure.External;

// ─── ChatGPT (OpenAI) ────────────────────────────────────────────────────────
public class ChatGPTService : IChatGPTService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public ChatGPTService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key missing");
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _http.BaseAddress = new Uri("https://api.openai.com/");
    }

    public async Task<string> GenerateScriptAsync(string topic, string niche)
    {
        var payload = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = $"You are an expert YouTube script writer specializing in {niche} content. Write engaging, well-structured scripts." },
                new { role = "user", content = $"Write a detailed YouTube video script for the topic: '{topic}'. Include intro hook, main content sections, and a call-to-action. Format it clearly with sections." }
            },
            max_tokens = 2000,
            temperature = 0.7
        };

        var response = await _http.PostAsJsonAsync("v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    public async Task<string> OptimizeSEOAsync(string title, string description, string? keywords)
    {
        var payload = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = "You are a YouTube SEO expert. Return JSON with: optimizedTitle, optimizedDescription, tags (array of 15 strings), seoScore (0-100)." },
                new { role = "user", content = $"Optimize for YouTube SEO:\nTitle: {title}\nDescription: {description}\nKeywords: {keywords ?? "auto-detect"}\nReturn ONLY valid JSON." }
            },
            max_tokens = 1000,
            temperature = 0.3
        };

        var response = await _http.PostAsJsonAsync("v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";
    }
}

// ─── Voiceover (ElevenLabs) ──────────────────────────────────────────────────
public class VoiceoverService : IVoiceoverService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _voiceId;

    public VoiceoverService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["ElevenLabs:ApiKey"] ?? throw new InvalidOperationException("ElevenLabs API key missing");
        _voiceId = config["ElevenLabs:VoiceId"] ?? "21m00Tcm4TlvDq8ikWAM";
        _http.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
        _http.BaseAddress = new Uri("https://api.elevenlabs.io/");
    }

    public async Task<string> GenerateVoiceoverAsync(string script)
    {
        // Truncate to 2500 chars for free tier
        var trimmedScript = script.Length > 2500 ? script[..2500] : script;
        var payload = new
        {
            text = trimmedScript,
            model_id = "eleven_monolingual_v1",
            voice_settings = new { stability = 0.5, similarity_boost = 0.75 }
        };

        var response = await _http.PostAsJsonAsync($"v1/text-to-speech/{_voiceId}", payload);
        response.EnsureSuccessStatusCode();

        var audioBytes = await response.Content.ReadAsByteArrayAsync();
        // In production: upload to Azure Blob; here return base64 data URL for simplicity
        var base64 = Convert.ToBase64String(audioBytes);
        return $"data:audio/mpeg;base64,{base64}";
    }
}

// ─── Video Generation (D-ID / Simulated) ────────────────────────────────────
public class VideoGenerationService : IVideoGenerationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public VideoGenerationService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> GenerateVideoAsync(string script, string title)
    {
        // D-ID API integration for AI avatar video
        var apiKey = _config["DID:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            // Return placeholder in dev mode
            await Task.Delay(2000);
            return $"https://placeholder.video/generated/{Guid.NewGuid()}.mp4";
        }

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Basic {apiKey}");
        _http.BaseAddress = new Uri("https://api.d-id.com/");

        var payload = new
        {
            script = new
            {
                type = "text",
                input = script.Length > 1000 ? script[..1000] : script,
                provider = new { type = "microsoft", voice_id = "en-US-JennyNeural" }
            },
            config = new { fluent = true, pad_audio = 0 },
            source_url = "https://d-id-public-bucket.s3.amazonaws.com/alice.jpg"
        };

        var response = await _http.PostAsJsonAsync("talks", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("id").GetString() ?? "";
    }
}

// ─── Thumbnail (DALL-E 3) ────────────────────────────────────────────────────
public class ThumbnailService : IThumbnailService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public ThumbnailService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["OpenAI:ApiKey"] ?? "";
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _http.BaseAddress = new Uri("https://api.openai.com/");
    }

    public async Task<string> GenerateThumbnailAsync(string title, string niche)
    {
        var payload = new
        {
            model = "dall-e-3",
            prompt = $"YouTube thumbnail for: '{title}' in {niche} niche. Bold text, vibrant colors, high contrast, professional, eye-catching. 16:9 ratio.",
            n = 1,
            size = "1792x1024",
            quality = "standard"
        };

        var response = await _http.PostAsJsonAsync("v1/images/generations", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("data")[0].GetProperty("url").GetString() ?? "";
    }
}
