/**
 * AGiXT C# SDK Tests
 *
 * These tests run against a live AGiXT server.
 * Set the following environment variables:
 * - AGIXT_URI: AGiXT server URI (default: http://localhost:7437)
 * - AGIXT_API_KEY: API key for authentication (default: test-api-key)
 */

using Xunit;
using AGiXT;

namespace AGiXT.SDK.Tests;

public class AGiXTSDKTests : IAsyncLifetime
{
    private readonly AGiXTSDK _sdk;
    private readonly string _agixtUri;
    private readonly string _apiKey;
    private string? _testAgentName;
    private string? _testAgentId;
    private string? _testConversationName;
    private string? _testConversationId;

    public AGiXTSDKTests()
    {
        _agixtUri = Environment.GetEnvironmentVariable("AGIXT_URI") ?? "http://localhost:7437";
        _apiKey = Environment.GetEnvironmentVariable("AGIXT_API_KEY") ?? "test-api-key";
        _sdk = new AGiXTSDK(baseUri: _agixtUri, apiKey: _apiKey);
    }

    public async Task InitializeAsync()
    {
        // Create test agent
        _testAgentName = $"TestAgent_{Guid.NewGuid().ToString("N")[..8]}";
        await _sdk.AddAgentAsync(_testAgentName, new Dictionary<string, object> { ["provider"] = "default" });
        _testAgentId = await _sdk.GetAgentIdByNameAsync(_testAgentName);

        // Create test conversation
        _testConversationName = $"TestConv_{Guid.NewGuid().ToString("N")[..8]}";
        await _sdk.NewConversationAsync(_testAgentId!, _testConversationName);
        _testConversationId = await _sdk.GetConversationIdByNameAsync(_testConversationName);
    }

    public async Task DisposeAsync()
    {
        // Cleanup
        try
        {
            if (!string.IsNullOrEmpty(_testConversationId))
            {
                await _sdk.DeleteConversationAsync(_testConversationId);
            }
            if (!string.IsNullOrEmpty(_testAgentId))
            {
                await _sdk.DeleteAgentAsync(_testAgentId);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task ServerIsReachable()
    {
        var providers = await _sdk.GetProvidersAsync();
        Assert.NotNull(providers);
    }

    [Fact]
    public async Task RegisterUser()
    {
        var email = $"test_{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var response = await _sdk.RegisterUserAsync(email, "Test", "User");
        Assert.NotNull(response);
    }

    [Fact]
    public async Task UserExists()
    {
        var email = $"existing_{Guid.NewGuid().ToString("N")[..8]}@example.com";
        await _sdk.RegisterUserAsync(email, "Existing", "User");
        var exists = await _sdk.UserExistsAsync(email);
        Assert.True(exists);
    }

    [Fact]
    public async Task GetAgents()
    {
        var agents = await _sdk.GetAgentsAsync();
        Assert.NotNull(agents);
    }

    [Fact]
    public async Task AddAgent()
    {
        var agentName = $"NewAgent_{Guid.NewGuid().ToString("N")[..8]}";
        var agent = await _sdk.AddAgentAsync(agentName, new Dictionary<string, object> { ["provider"] = "default" });
        Assert.NotNull(agent);

        // Cleanup
        var agentId = await _sdk.GetAgentIdByNameAsync(agentName);
        if (!string.IsNullOrEmpty(agentId))
        {
            await _sdk.DeleteAgentAsync(agentId);
        }
    }

    [Fact]
    public async Task GetAgentIdByName()
    {
        var agentId = await _sdk.GetAgentIdByNameAsync(_testAgentName!);
        Assert.NotNull(agentId);
        Assert.Equal(_testAgentId, agentId);
    }

    [Fact]
    public async Task GetAgentConfig()
    {
        var config = await _sdk.GetAgentConfigAsync(_testAgentId!);
        Assert.NotNull(config);
    }

    [Fact]
    public async Task GetConversations()
    {
        var conversations = await _sdk.GetConversationsAsync();
        Assert.NotNull(conversations);
    }

    [Fact]
    public async Task NewConversation()
    {
        var convName = $"NewConv_{Guid.NewGuid().ToString("N")[..8]}";
        var conv = await _sdk.NewConversationAsync(_testAgentId!, convName);
        Assert.NotNull(conv);

        // Cleanup
        var convId = await _sdk.GetConversationIdByNameAsync(convName);
        if (!string.IsNullOrEmpty(convId))
        {
            await _sdk.DeleteConversationAsync(convId);
        }
    }

    [Fact]
    public async Task GetConversationIdByName()
    {
        var convId = await _sdk.GetConversationIdByNameAsync(_testConversationName!);
        Assert.NotNull(convId);
        Assert.Equal(_testConversationId, convId);
    }

    [Fact]
    public async Task GetConversation()
    {
        var history = await _sdk.GetConversationAsync(_testConversationId!);
        Assert.NotNull(history);
    }

    [Fact]
    public async Task GetProviders()
    {
        var providers = await _sdk.GetProvidersAsync();
        Assert.NotNull(providers);
    }

    [Fact]
    public async Task GetProvidersByService()
    {
        var providers = await _sdk.GetProvidersByServiceAsync("llm");
        Assert.NotNull(providers);
    }

    [Fact]
    public async Task GetChains()
    {
        var chains = await _sdk.GetChainsAsync();
        Assert.NotNull(chains);
    }

    [Fact]
    public async Task GetPrompts()
    {
        var prompts = await _sdk.GetPromptsAsync();
        Assert.NotNull(prompts);
    }

    [Fact]
    public async Task GetAllPrompts()
    {
        var prompts = await _sdk.GetAllPromptsAsync();
        Assert.NotNull(prompts);
    }

    [Fact]
    public async Task GetExtensions()
    {
        var extensions = await _sdk.GetExtensionsAsync();
        Assert.NotNull(extensions);
    }
}
