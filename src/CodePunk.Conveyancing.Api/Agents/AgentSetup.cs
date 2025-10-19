using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.AI;
// Provider extensions are brought in via Microsoft.Extensions.AI.* packages

namespace CodePunk.Conveyancing.Api.Agents;

public static class AgentSetup
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        // For now, register a placeholder chat client until provider config is supplied.
        services.AddSingleton<IChatClient, NullChatClient>();

        services.AddSingleton<ITitleEnquiryAgent, TitleEnquiryAgent>();

        return services;
    }

    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/agents");
        group.MapGet("/status", () => Results.Ok(new { ready = true }));
        return routes;
    }
}

internal sealed class NullChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var text = "AI provider not configured";
        var message = new ChatMessage(ChatRole.Assistant, text);
        return Task.FromResult(new ChatResponse(message));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        async IAsyncEnumerable<ChatResponseUpdate> Iterator()
        {
            await Task.CompletedTask;
            yield break;
        }
        return Iterator();
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
