# Quiz Investigativo Grupo 3 - Microservicios .NET

Este proyecto contiene dos Web APIs independientes en .NET 8:

- `UsuariosService`: corre en `http://localhost:5001` y administra usuarios.
- `PedidosService`: corre en `http://localhost:5002` y administra pedidos.

La comunicacion entre servicios ocurre solamente cuando `PedidosService` valida por HTTP que el usuario exista antes de crear un pedido.

## Como ejecutar

Si descargaron el proyecto desde GitHub:

`powershell
git clone URL_DEL_REPOSITORIO
cd QuizIventigativoG3
` 

Tambien pueden abrir `QuizInvestigativoG3.slnx` desde Visual Studio.

En una terminal:

```powershell
dotnet run --project .\UsuariosService\UsuariosService.csproj
```

En otra terminal:

```powershell
dotnet run --project .\PedidosService\PedidosService.csproj
```

## Pruebas rapidas

Requisito: tener instalados `.NET 8 SDK` y Git.

Listar usuarios:

```powershell
Invoke-RestMethod http://localhost:5001/api/usuarios
```

Buscar usuario por id:

```powershell
Invoke-RestMethod http://localhost:5001/api/usuarios/1
```

Listar pedidos:

```powershell
Invoke-RestMethod http://localhost:5002/api/pedidos
```

Crear pedido con usuario existente:

```powershell
Invoke-RestMethod http://localhost:5002/api/pedidos `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"usuarioId":1,"producto":"Mouse inalambrico","cantidad":2}'
```

Crear pedido con usuario inexistente:

```powershell
Invoke-RestMethod http://localhost:5002/api/pedidos `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"usuarioId":99,"producto":"Teclado","cantidad":1}'
```

## Explicacion por archivo

### UsuariosService/UsuariosService.csproj

Define a `UsuariosService` como un proyecto Web API independiente en .NET 8.
Usa `Microsoft.NET.Sdk.Web` porque este servicio expone endpoints HTTP.
En microservicios, cada servicio debe poder compilarse y ejecutarse por separado.
Este archivo representa esa independencia tecnica del servicio de usuarios.

### UsuariosService/Models/Usuario.cs

Contiene el modelo interno que usa `UsuariosService` para representar usuarios.
Solo este servicio conoce y usa esta clase, porque la gestion de usuarios es su responsabilidad.
No se comparte con `PedidosService`, lo cual evita acoplamiento entre proyectos.
Eso mantiene la separacion real entre dominios.

### UsuariosService/Program.cs

Configura el servicio para correr en el puerto `5001`.
Mantiene una lista en memoria de usuarios y expone `GET /api/usuarios` y `GET /api/usuarios/{id}`.
Este servicio no sabe nada de pedidos; solo responde informacion de usuarios.
Esa responsabilidad unica es una idea central de microservicios.

### PedidosService/PedidosService.csproj

Define a `PedidosService` como otro proyecto Web API independiente en .NET 8.
No referencia el `.csproj` de `UsuariosService`, por lo que no depende directamente de su codigo.
Esto permite explicar que son servicios separados, no carpetas dentro de una misma aplicacion.
La comunicacion entre ambos se hace por HTTP, no por clases compartidas.

### PedidosService/Models/Pedido.cs

Representa el modelo interno de pedidos.
Incluye `UsuarioId`, pero no incluye un objeto `Usuario`, porque pedidos no administra usuarios.
Eso muestra que cada microservicio conserva sus propios datos y solo guarda las referencias necesarias.
La informacion completa del usuario se consulta al servicio correspondiente.

### PedidosService/Models/CrearPedidoRequest.cs

Representa los datos que el cliente envia para crear un pedido.
Separarlo de `Pedido` evita que el cliente controle campos internos como `Id` o `FechaCreacion`.
Esta separacion mejora el diseno de la API y deja claro que el servicio decide como crear el recurso.
Es una practica comun en APIs y microservicios.

### PedidosService/Dtos/UsuarioDto.cs

Es el contrato local que `PedidosService` espera recibir desde `UsuariosService`.
No reutiliza la clase `Usuario` del otro proyecto.
Esto es importante porque un microservicio no debe depender de las clases internas de otro.
El DTO permite comunicacion HTTP sin acoplar el codigo fuente.

### PedidosService/Clients/UsuariosClient.cs

Encapsula la llamada HTTP desde `PedidosService` hacia `UsuariosService`.
El metodo `ObtenerUsuarioPorIdAsync` llama a `/api/usuarios/{id}` y devuelve `null` si el usuario no existe.
Esto centraliza la comunicacion externa en una clase clara y facil de explicar.
Asi el endpoint de pedidos no queda mezclado con detalles de bajo nivel de HTTP.

### PedidosService/Program.cs

Configura el servicio para correr en el puerto `5002`.
Registra el `HttpClient` tipado, expone `GET /api/pedidos` y `POST /api/pedidos`.
En el `POST`, antes de crear un pedido, llama a `UsuariosService` para validar que el usuario exista.
Si el usuario no existe, responde `400 Bad Request`.

## Flecha de comunicacion entre servicios

La flecha arquitectonica es:

```text
PedidosService ---> HTTP GET /api/usuarios/{id} ---> UsuariosService
```

En codigo aparece en dos lugares:

1. Registro del cliente HTTP en `PedidosService/Program.cs`:

```csharp
builder.Services.AddHttpClient<UsuariosClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});
```

2. Uso del cliente dentro del `POST /api/pedidos`:

```csharp
var usuario = await usuariosClient.ObtenerUsuarioPorIdAsync(request.UsuarioId);
```

Esa es la parte clave para explicar en la exposicion: `PedidosService` no usa clases ni base de datos de `UsuariosService`; le pregunta por HTTP si el usuario existe.

## Que debe explicar cada integrante

- Que es un microservicio y por que aqui hay dos servicios separados.
- Que responsabilidad tiene `UsuariosService`.
- Que responsabilidad tiene `PedidosService`.
- Por que cada proyecto tiene su propio `.csproj` y su propio puerto.
- Por que `PedidosService` no reutiliza la clase `Usuario`.
- Donde esta el DTO `UsuarioDto` y para que sirve.
- Donde esta la flecha HTTP entre servicios.
- Que pasa cuando se intenta crear un pedido con un usuario inexistente.
- Por que se usan listas en memoria para el quiz y no una base de datos real.
- Que limitaciones tiene este ejemplo frente a microservicios de produccion.

## Nota sobre representatividad

El diseno es representativo para un quiz porque muestra servicios separados, responsabilidad unica, puertos distintos y comunicacion HTTP.
La limitacion principal es que en produccion normalmente habria bases de datos separadas, observabilidad, tolerancia a fallos, autenticacion, configuracion externa y despliegue independiente.
No se agregaron esas piezas porque el alcance indicado es academico y de 3 horas.

