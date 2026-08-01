using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UsuariosService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5001");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<UsuariosSwaggerExamples>();
});

var app = builder.Build();

var usuarios = new List<Usuario>
{
    new() { Id = 1, Nombre = "Ana Rodriguez", Correo = "ana@demo.com" },
    new() { Id = 2, Nombre = "Carlos Mora", Correo = "carlos@demo.com" },
    new() { Id = 3, Nombre = "Mariana Vargas", Correo = "mariana@demo.com" }
};

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/usuarios", () => Results.Ok(usuarios))
    .Produces<List<Usuario>>(StatusCodes.Status200OK);

app.MapGet("/api/usuarios/{id:int}", (int id) =>
{
    var usuario = usuarios.FirstOrDefault(u => u.Id == id);

    return usuario is null
        ? Results.NotFound()
        : Results.Ok(usuario);
})
    .Produces<Usuario>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

public class UsuariosSwaggerExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        var method = context.ApiDescription.HttpMethod;

        if (method == "GET" && path == "api/usuarios")
        {
            operation.Summary = "Lista todos los usuarios";
            operation.Description = "Este GET no necesita parametros ni body JSON.";
            AddJsonResponseExample(operation, "200", new OpenApiArray
            {
                UsuarioExample(1, "Ana Rodriguez", "ana@demo.com"),
                UsuarioExample(2, "Carlos Mora", "carlos@demo.com"),
                UsuarioExample(3, "Mariana Vargas", "mariana@demo.com")
            });
        }

        if (method == "GET" && path == "api/usuarios/{id:int}")
        {
            operation.Summary = "Busca un usuario por Id";
            operation.Description = "Usa el parametro id. Ejemplo recomendado: id = 1.";

            var idParameter = operation.Parameters.FirstOrDefault(parameter => parameter.Name == "id");
            if (idParameter is not null)
            {
                idParameter.Description = "Id del usuario. Ejemplo: 1.";
                idParameter.Example = new OpenApiInteger(1);
            }

            AddJsonResponseExample(operation, "200", UsuarioExample(1, "Ana Rodriguez", "ana@demo.com"));
        }
    }

    private static OpenApiObject UsuarioExample(int id, string nombre, string correo) => new()
    {
        ["id"] = new OpenApiInteger(id),
        ["nombre"] = new OpenApiString(nombre),
        ["correo"] = new OpenApiString(correo)
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
