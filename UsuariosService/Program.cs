using UsuariosService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5001");

var app = builder.Build();

var usuarios = new List<Usuario>
{
    new() { Id = 1, Nombre = "Ana Rodriguez", Correo = "ana@demo.com" },
    new() { Id = 2, Nombre = "Carlos Mora", Correo = "carlos@demo.com" },
    new() { Id = 3, Nombre = "Mariana Vargas", Correo = "mariana@demo.com" }
};

app.MapGet("/api/usuarios", () => Results.Ok(usuarios));

app.MapGet("/api/usuarios/{id:int}", (int id) =>
{
    var usuario = usuarios.FirstOrDefault(u => u.Id == id);

    return usuario is null
        ? Results.NotFound()
        : Results.Ok(usuario);
});

app.Run();
