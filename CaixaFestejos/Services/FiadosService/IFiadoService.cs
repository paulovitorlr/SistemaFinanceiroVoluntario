using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

public interface IFiadoService
{
    void RegistrarFiado(
        string cliente,
        List<ItemPedido> pedido);
}