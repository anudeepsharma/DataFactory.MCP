using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Configuration;
using DataFactory.MCP.Extensions;
using DataFactory.MCP.Infrastructure.Http;
using DataFactory.MCP.Models.Azure;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;

namespace DataFactory.MCP.Services;

/// <summary>
/// Service for discovering Azure resources using Azure Resource Manager APIs.
/// Authentication is handled automatically by the AzureResourceManagerAuthenticationHandler in the HTTP pipeline.
/// Implements request-level caching to reduce API quota usage and improve performance.
/// </summary>
public class AzureResourceDiscoveryService : IAzureResourceDiscoveryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureResourceDiscoveryService> _logger;
    private readonly ConcurrentDictionary<string, CachedResourceData> _cache = new();
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    private static JsonSerializerOptions JsonOptions => JsonSerializerOptionsProvider.CaseInsensitive;

    /// <summary>
    /// Represents cached resource data with expiration timestamp
    /// </summary>
    private sealed class CachedResourceData
    {
        public object Data { get; init; } = null!;
        public DateTime ExpiresAt { get; init; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }

    public AzureResourceDiscoveryService(
        IHttpClientFactory httpClientFactory,
        ILogger<AzureResourceDiscoveryService> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientNames.AzureResourceManager);
        _logger = logger;
    }

    public async Task<List<AzureSubscription>> GetSubscriptionsAsync()
    {
        const string cacheKeyPrefix = "subscriptions";
        
        if (TryGetFromCache<List<AzureSubscription>>(cacheKeyPrefix, out var cachedSubscriptions))
        {
            _logger.LogInformation("Returning cached subscriptions (cached {Count} items)", cachedSubscriptions!.Count);
            return cachedSubscriptions;
        }

        try
        {
            _logger.LogInformation("Fetching Azure subscriptions from Azure Resource Manager");

            var url = FabricUrlBuilder.ForAzureResourceManager()
                .WithLiteralPath("subscriptions")
                .WithApiVersion(ApiVersions.AzureResourceManager.Subscriptions)
                .Build();

            var response = await _httpClient.GetAsync(url);
            var subscriptionsResponse = await response.ReadAsJsonOrDefaultAsync(
                new AzureSubscriptionsResponse(), JsonOptions);

            var subscriptions = subscriptionsResponse.Value ?? new List<AzureSubscription>();
            _logger.LogInformation("Successfully retrieved {Count} subscriptions", subscriptions.Count);
            
            AddToCache(cacheKeyPrefix, subscriptions, DefaultCacheDuration);
            return subscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure subscriptions");
            return new List<AzureSubscription>();
        }
    }

    public async Task<List<AzureResourceGroup>> GetResourceGroupsAsync(string subscriptionId)
    {
        var cacheKeyPrefix = $"resourcegroups:{subscriptionId}";
        
        if (TryGetFromCache<List<AzureResourceGroup>>(cacheKeyPrefix, out var cachedResourceGroups))
        {
            _logger.LogInformation("Returning cached resource groups for subscription {SubscriptionId} (cached {Count} items)",
                subscriptionId, cachedResourceGroups!.Count);
            return cachedResourceGroups;
        }

        try
        {
            _logger.LogInformation("Fetching resource groups for subscription {SubscriptionId} from Azure Resource Manager", subscriptionId);

            var url = FabricUrlBuilder.ForAzureResourceManager()
                .WithLiteralPath($"subscriptions/{subscriptionId}/resourcegroups")
                .WithApiVersion(ApiVersions.AzureResourceManager.ResourceGroups)
                .Build();

            var response = await _httpClient.GetAsync(url);
            var resourceGroupsResponse = await response.ReadAsJsonOrDefaultAsync(
                new AzureResourceGroupsResponse(), JsonOptions);

            var resourceGroups = resourceGroupsResponse.Value ?? new List<AzureResourceGroup>();
            _logger.LogInformation("Successfully retrieved {Count} resource groups", resourceGroups.Count);
            
            AddToCache(cacheKeyPrefix, resourceGroups, DefaultCacheDuration);
            return resourceGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource groups for subscription {SubscriptionId}", subscriptionId);
            return new List<AzureResourceGroup>();
        }
    }

    public async Task<List<AzureVirtualNetwork>> GetVirtualNetworksAsync(string subscriptionId, string? resourceGroupName = null)
    {
        var cacheKeyPrefix = $"vnets:{subscriptionId}:{resourceGroupName ?? "all"}";
        
        if (TryGetFromCache<List<AzureVirtualNetwork>>(cacheKeyPrefix, out var cachedVNets))
        {
            _logger.LogInformation("Returning cached virtual networks for subscription {SubscriptionId}, resource group {ResourceGroupName} (cached {Count} items)",
                subscriptionId, resourceGroupName ?? "all", cachedVNets!.Count);
            return cachedVNets;
        }

        try
        {
            _logger.LogInformation("Fetching virtual networks for subscription {SubscriptionId}, resource group {ResourceGroupName} from Azure Resource Manager",
                subscriptionId, resourceGroupName ?? "all");

            var urlBuilder = FabricUrlBuilder.ForAzureResourceManager();
            if (!string.IsNullOrEmpty(resourceGroupName))
            {
                urlBuilder.WithLiteralPath($"subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Network/virtualNetworks");
            }
            else
            {
                urlBuilder.WithLiteralPath($"subscriptions/{subscriptionId}/providers/Microsoft.Network/virtualNetworks");
            }
            var url = urlBuilder.WithApiVersion(ApiVersions.AzureResourceManager.Network).Build();

            var response = await _httpClient.GetAsync(url);
            var virtualNetworksResponse = await response.ReadAsJsonOrDefaultAsync(
                new AzureVirtualNetworksResponse(), JsonOptions);

            var virtualNetworks = virtualNetworksResponse.Value ?? new List<AzureVirtualNetwork>();
            _logger.LogInformation("Successfully retrieved {Count} virtual networks", virtualNetworks.Count);
            
            AddToCache(cacheKeyPrefix, virtualNetworks, DefaultCacheDuration);
            return virtualNetworks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting virtual networks for subscription {SubscriptionId}", subscriptionId);
            return new List<AzureVirtualNetwork>();
        }
    }

    public async Task<List<AzureSubnet>> GetSubnetsAsync(string subscriptionId, string resourceGroupName, string virtualNetworkName)
    {
        var cacheKeyPrefix = $"subnets:{subscriptionId}:{resourceGroupName}:{virtualNetworkName}";
        
        if (TryGetFromCache<List<AzureSubnet>>(cacheKeyPrefix, out var cachedSubnets))
        {
            _logger.LogInformation("Returning cached subnets for VNet {VirtualNetworkName} in resource group {ResourceGroupName} (cached {Count} items)",
                virtualNetworkName, resourceGroupName, cachedSubnets!.Count);
            return cachedSubnets;
        }

        try
        {
            _logger.LogInformation("Fetching subnets for VNet {VirtualNetworkName} in resource group {ResourceGroupName} from Azure Resource Manager",
                virtualNetworkName, resourceGroupName);

            var url = FabricUrlBuilder.ForAzureResourceManager()
                .WithLiteralPath($"subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Network/virtualNetworks/{virtualNetworkName}/subnets")
                .WithApiVersion(ApiVersions.AzureResourceManager.Network)
                .Build();

            var response = await _httpClient.GetAsync(url);
            var subnetsResponse = await response.ReadAsJsonOrDefaultAsync(
                new AzureSubnetsResponse(), JsonOptions);

            var subnets = subnetsResponse.Value ?? new List<AzureSubnet>();
            _logger.LogInformation("Successfully retrieved {Count} subnets", subnets.Count);
            
            AddToCache(cacheKeyPrefix, subnets, DefaultCacheDuration);
            return subnets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subnets for VNet {VirtualNetworkName}", virtualNetworkName);
            return new List<AzureSubnet>();
        }
    }

    /// <summary>
    /// Attempts to retrieve a cached value if it exists and hasn't expired
    /// </summary>
    private bool TryGetFromCache<T>(string cacheKey, out T? cachedValue)
    {
        if (_cache.TryGetValue(cacheKey, out var cachedData) && !cachedData.IsExpired)
        {
            cachedValue = (T)cachedData.Data;
            return true;
        }

        cachedValue = default;
        return false;
    }

    /// <summary>
    /// Adds or updates a value in the cache with the specified TTL
    /// </summary>
    private void AddToCache(string cacheKey, object value, TimeSpan cacheDuration)
    {
        var cachedData = new CachedResourceData
        {
            Data = value,
            ExpiresAt = DateTime.UtcNow.Add(cacheDuration)
        };

        _cache.AddOrUpdate(cacheKey, cachedData, (_, __) => cachedData);
    }
}