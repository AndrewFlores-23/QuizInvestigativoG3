namespace PedidosService.Models;

public class CrearPedidoRequest
{
    public int UsuarioId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
