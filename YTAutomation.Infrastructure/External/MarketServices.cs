using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YTAutomation.Core.DTOs;
using YTAutomation.Core.Interfaces;

namespace YTAutomation.Infrastructure.External;

// ─── Grok (xAI) ──────────────────────────────────────────────────────────────
public class GrokService : IGrokService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GrokService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Grok:ApiKey"] ?? "";
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _http.BaseAddress = new Uri("https://api.x.ai/");
    }

    public async Task<MarketInsightDto> AnalyzeTrendsAsync(string niche)
    {
        var payload = new
        {
            model = "grok-2-latest",
            messages = new[]
            {
                new { role = "system", content = "You are a YouTube market analyst. Return a JSON object with: trendingTopics (array of 10 strings), analysisSummary (string), trendScore (0-100 number)." },
                new { role = "user", content = $"What are the top trending YouTube video ideas right now for the '{niche}' niche? What's working? What should a creator make today? Return ONLY valid JSON." }
            },
            temperature = 0.5
        };

        try
        {
            var response = await _http.PostAsJsonAsync("v1/chat/completions", payload);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var content = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";
            return ParseMarketInsight(niche, content, "grok");
        }
        catch
        {
            return GetFallbackInsight(niche, "grok");
        }
    }

    private static MarketInsightDto ParseMarketInsight(string niche, string json, string source)
    {
        try
        {
            var cleanJson = json.Trim();
            if (cleanJson.StartsWith("```json")) cleanJson = cleanJson[7..];
            if (cleanJson.EndsWith("```")) cleanJson = cleanJson[..^3];

            var data = JsonSerializer.Deserialize<JsonElement>(cleanJson.Trim());
            var topics = data.GetProperty("trendingTopics").EnumerateArray().Select(t => t.GetString() ?? "").ToList();
            var summary = data.GetProperty("analysisSummary").GetString() ?? "";
            var score = data.GetProperty("trendScore").GetDouble();

            return new MarketInsightDto(0, niche, topics, summary, source, score, DateTime.UtcNow);
        }
        catch
        {
            return GetFallbackInsight(niche, source);
        }
    }

    private static MarketInsightDto GetFallbackInsight(string niche, string source) =>
        new(0, niche,
            new List<string> { $"Top {niche} trends 2025", $"{niche} beginner guide", $"{niche} tips and tricks" },
            $"Market analysis for {niche} is trending upward.",
            source, 75.0, DateTime.UtcNow);
}

// ─── Gemini (Google) ─────────────────────────────────────────────────────────
public class GeminiService : IGeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Gemini:ApiKey"] ?? "";
        _http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    }

    public async Task<MarketInsightDto> AnalyzeTrendsAsync(string niche)
    {
        var url = $"v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"As a YouTube market analyst, analyze the '{niche}' niche. Return ONLY a JSON object with: trendingTopics (array of 10 strings), analysisSummary (string under 200 words), trendScore (number 0-100)." }
                    }
                }
            },
            generationConfig = new { temperature = 0.4, maxOutputTokens = 800 }
        };

        try
        {
            var response = await _http.PostAsJsonAsync(url, payload);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var content = result.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "{}";
            return ParseMarketInsight(niche, content, "gemini");
        }
        catch
        {
            return GetFallbackInsight(niche, "gemini");
        }
    }

    private static MarketInsightDto ParseMarketInsight(string niche, string json, string source)
    {
        try
        {
            var cleanJson = json.Trim();
            if (cleanJson.Contains("```json")) cleanJson = cleanJson[(cleanJson.IndexOf("```json") + 7)..];
            if (cleanJson.Contains("```")) cleanJson = cleanJson[..cleanJson.LastIndexOf("```")];

            var data = JsonSerializer.Deserialize<JsonElement>(cleanJson.Trim());
            var topics = data.GetProperty("trendingTopics").EnumerateArray().Select(t => t.GetString() ?? "").ToList();
            var summary = data.GetProperty("analysisSummary").GetString() ?? "";
            var score = data.GetProperty("trendScore").GetDouble();

            return new MarketInsightDto(0, niche, topics, summary, source, score, DateTime.UtcNow);
        }
        catch
        {
            return GetFallbackInsight(niche, source);
        }
    }

    private static MarketInsightDto GetFallbackInsight(string niche, string source) =>
        new(0, niche,
            new List<string> { $"Latest {niche} developments", $"{niche} for beginners", $"{niche} advanced guide" },
            $"The {niche} niche shows strong audience engagement and growth potential.",
            source, 80.0, DateTime.UtcNow);
}
