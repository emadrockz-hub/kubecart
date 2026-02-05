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

    public async Task<bool> DecreaseStockAsync(int productId, int quantity, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(
            $"/api/catalog/products/{productId}/decrease-stock",
            new { quantity },
            ct);

        // SUCCESS
        if ((int)resp.StatusCode == 204) return true;

        // Add visibility for debugging
        var body = await resp.Content.ReadAsStringAsync(ct);

        // 404/409 = business failure
        if ((int)resp.StatusCode == 404 || (int)resp.StatusCode == 409)
        {
            
        }

        // Unexpected → throw so we see it
        resp.EnsureSuccessStatusCode();
        return false;
    }
}