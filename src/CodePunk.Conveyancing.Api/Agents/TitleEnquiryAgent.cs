using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CodePunk.Conveyancing.Api.Agents;

public interface ITitleEnquiryAgent
{
    Task<AgentDraftResult> DraftEnquiriesAsync(string propertyAddress, CancellationToken ct = default);
}

public sealed record AgentDraftResult(string Content, IReadOnlyDictionary<string, string> Audit);

internal sealed class TitleEnquiryAgent : ITitleEnquiryAgent
{
    private readonly IChatClient _chat;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public TitleEnquiryAgent(IChatClient chat, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _chat = chat;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<AgentDraftResult> DraftEnquiriesAsync(string propertyAddress, CancellationToken ct = default)
    {
        // Prefer Anthropic if configured
        var anthropicKey = _config["AI:Anthropic:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (!string.IsNullOrWhiteSpace(anthropicKey))
        {
            var model = _config["AI:Anthropic:ModelId"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-3-5-sonnet-latest";
            return await DraftWithAnthropicAsync(propertyAddress, anthropicKey!, model, ct);
        }

        // Fallback to Microsoft.Extensions.AI client if available
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a UK residential conveyancing solicitor. Draft a concise list of initial enquiries for the buyer’s solicitor to raise, based only on the address. If information is missing, state reasonable assumptions clearly."),
            new(ChatRole.User, $"Property address: {propertyAddress}\nOutput format: bullet list (markdown).")
        };

        var sw = Stopwatch.StartNew();
        ChatResponse response = await _chat.GetResponseAsync(messages, cancellationToken: ct);
        sw.Stop();

        var content = response.Text ?? string.Empty;
        var audit = new Dictionary<string, string>
        {
            ["provider"] = _config["AI:Provider"] ?? "extensions.ai",
            ["model"] = _config["AI:OpenAI:ModelId"] ?? _config["AI:AzureAIInference:ModelId"] ?? string.Empty,
            ["agent"] = nameof(TitleEnquiryAgent),
            ["latency_ms"] = sw.ElapsedMilliseconds.ToString(),
            ["prompt_hash"] = ComputeSha256($"system:{messages[0].Text}|user:{messages[1].Text}")
        };
        return new AgentDraftResult(content, audit);
    }

    private async Task<AgentDraftResult> DraftWithAnthropicAsync(string propertyAddress, string apiKey, string model, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");

        var system = "You are a UK residential conveyancing solicitor. Draft a concise list of initial enquiries for the buyer’s solicitor to raise, based only on the address. If information is missing, state reasonable assumptions clearly.";
        var user = $"Property address: {propertyAddress}\nOutput format: bullet list (markdown).";

        var body = new
        {
            model,
            max_tokens = 800,
            system,
            messages = new[]
            {
                new { role = "user", content = user }
            }
        };

        var json = JsonSerializer.Serialize(body);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        using var resp = await client.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        sw.Stop();

        // Extract text from content blocks
        if (doc.RootElement.TryGetProperty("content", out var contentElem) && contentElem.ValueKind == JsonValueKind.Array)
        {
            var sb = new StringBuilder();
            foreach (var block in contentElem.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var type) && type.GetString() == "text" &&
                    block.TryGetProperty("text", out var text))
                {
                    sb.AppendLine(text.GetString());
                }
            }
            var content = sb.ToString().Trim();

            int? inTok = null, outTok = null;
            if (doc.RootElement.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                if (usage.TryGetProperty("input_tokens", out var it) && it.TryGetInt32(out var iv)) inTok = iv;
                if (usage.TryGetProperty("output_tokens", out var ot) && ot.TryGetInt32(out var ov)) outTok = ov;
            }

            var audit = new Dictionary<string, string>
            {
                ["provider"] = "anthropic",
                ["model"] = model,
                ["agent"] = nameof(TitleEnquiryAgent),
                ["latency_ms"] = sw.ElapsedMilliseconds.ToString(),
                ["prompt_hash"] = ComputeSha256($"system:{system}|user:{user}")
            };
            if (inTok.HasValue) audit["input_tokens"] = inTok.Value.ToString();
            if (outTok.HasValue) audit["output_tokens"] = outTok.Value.ToString();

            return new AgentDraftResult(content, audit);
        }

        var fallbackAudit = new Dictionary<string, string>
        {
            ["provider"] = "anthropic",
            ["model"] = model,
            ["agent"] = nameof(TitleEnquiryAgent),
            ["latency_ms"] = sw.ElapsedMilliseconds.ToString(),
            ["prompt_hash"] = ComputeSha256($"system:{system}|user:{user}")
        };
        return new AgentDraftResult(string.Empty, fallbackAudit);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
