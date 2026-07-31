namespace PedidosService.Models;

public class Pedido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime FechaCreacion { get; set; }
}
