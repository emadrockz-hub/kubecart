using System.Net.Http.Json;

namespace KubeCart.Orders.Api.Clients;

public sealed class CatalogClient
{
    private readonly HttpClient _http;

    public CatalogClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CatalogProductDto?> GetProductAsync(int productId, CancellationToken ct)
    {
        return await _http.GetFromJsonAsync<CatalogProductDto>(
            $"/api/catalog/products/{productId}",
            ct
        );
    }

    public async Task<IReadOnlyList<CatalogProductDto>> GetProductsAsync(CancellationToken ct)
    {
        var list = await _http.GetFromJsonAsync<List<CatalogProductDto>>(
            "/api/catalog/products",
            ct
        );

        return list ?? new List<CatalogProductDto>();
    }
}
