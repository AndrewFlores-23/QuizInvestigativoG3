using PedidosService.Clients;
using PedidosService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5002");

builder.Services.AddHttpClient<UsuariosClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});

var app = builder.Build();

var pedidos = new List<Pedido>
{
    new()
    {
        Id = 1,
        UsuarioId = 1,
        Producto = "Libro de arquitectura web",
        Cantidad = 1,
        FechaCreacion = DateTime.UtcNow
    }
};

var siguienteId = pedidos.Count + 1;

app.MapGet("/api/pedidos", () => Results.Ok(pedidos));

app.MapPost("/api/pedidos", async (CrearPedidoRequest request, UsuariosClient usuariosClient) =>
{
    if (request.UsuarioId <= 0 || string.IsNullOrWhiteSpace(request.Producto) || request.Cantidad <= 0)
    {
        return Results.BadRequest("Los datos del pedido no son validos.");
    }

    var usuario = await usuariosClient.ObtenerUsuarioPorIdAsync(request.UsuarioId);

    if (usuario is null)
    {
        return Results.BadRequest("No se puede crear el pedido porque el usuario no existe.");
    }

    var pedido = new Pedido
    {
        Id = siguienteId++,
        UsuarioId = request.UsuarioId,
        Producto = request.Producto,
        Cantidad = request.Cantidad,
        FechaCreacion = DateTime.UtcNow
    };

    pedidos.Add(pedido);

    return Results.Created($"/api/pedidos/{pedido.Id}", pedido);
});

app.Run();
