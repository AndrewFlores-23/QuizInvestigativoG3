using System.Net;
using System.Net.Http.Json;
using PedidosService.Dtos;

namespace PedidosService.Clients;

public class UsuariosClient
{
    private readonly HttpClient _httpClient;

    public UsuariosClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UsuarioDto?> ObtenerUsuarioPorIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/usuarios/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UsuarioDto>();
    }
}
