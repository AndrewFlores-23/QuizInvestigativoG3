using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PedidosService.Clients;
using PedidosService.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5002");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<PedidosSwaggerExamples>();
});

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/pedidos", () => Results.Ok(pedidos))
    .Produces<List<Pedido>>(StatusCodes.Status200OK);

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
})
    .Accepts<CrearPedidoRequest>("application/json")
    .Produces<Pedido>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.Run();

public class PedidosSwaggerExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        var method = context.ApiDescription.HttpMethod;

        if (method == "GET" && path == "api/pedidos")
        {
            operation.Summary = "Lista todos los pedidos";
            operation.Description = "Este GET no necesita parametros ni body JSON.";
            AddJsonResponseExample(operation, "200", new OpenApiArray
            {
                PedidoExample(1, 1, "Libro de arquitectura web", 1)
            });
        }

        if (method == "POST" && path == "api/pedidos")
        {
            operation.Summary = "Crea un pedido";
            operation.Description = "Usa este body JSON. El usuarioId debe existir en UsuariosService; por ejemplo, usuarioId = 1.";

            if (operation.RequestBody?.Content.TryGetValue("application/json", out var requestContent) == true)
            {
                requestContent.Example = new OpenApiObject
                {
                    ["usuarioId"] = new OpenApiInteger(1),
                    ["producto"] = new OpenApiString("Teclado mecanico"),
                    ["cantidad"] = new OpenApiInteger(2)
                };
            }

            AddJsonResponseExample(operation, "201", PedidoExample(2, 1, "Teclado mecanico", 2));
            AddJsonResponseExample(operation, "400", new OpenApiString("No se puede crear el pedido porque el usuario no existe."));
        }
    }

    private static OpenApiObject PedidoExample(int id, int usuarioId, string producto, int cantidad) => new()
    {
        ["id"] = new OpenApiInteger(id),
        ["usuarioId"] = new OpenApiInteger(usuarioId),
        ["producto"] = new OpenApiString(producto),
        ["cantidad"] = new OpenApiInteger(cantidad),
        ["fechaCreacion"] = new OpenApiString("2026-08-01T01:07:04.7757032Z")
    };

    private static void AddJsonResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = "Ejemplo" };
            operation.Responses[statusCode] = response;
        }

        if (!response.Content.TryGetValue("application/json", out var content))
        {
            content = new OpenApiMediaType();
            response.Content["application/json"] = content;
        }

        content.Example = example;
    }
}
