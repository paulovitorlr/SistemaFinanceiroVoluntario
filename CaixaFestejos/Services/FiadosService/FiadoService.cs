using CaixaFestejos.Models;
using CaixaFestejos.Repositories;

namespace CaixaFestejos.Services;

public class FiadoService : IFiadoService
{
    private readonly IFiadoRepository _fiadoRepository;

    public FiadoService(IFiadoRepository fiadoRepository)
    {
        _fiadoRepository = fiadoRepository;
    }

    public void RegistrarFiado(
        string cliente,
        List<ItemPedido> pedido)
    {
        var fiado = new Fiado
        {
            Cliente = cliente,
            DataCriacao = DateTime.Now,
            ValorTotal = pedido.Sum(x => x.Subtotal),
            Pago = false,

            Itens = pedido.Select(x => new ItemFiado
            {
                ProdutoId = x.ProdutoId,
                Nome = x.Nome,
                Quantidade = x.Quantidade,
                PrecoUnitario = x.Preco,
                CustoUnitario = x.Custo
            }).ToList()
        };

        _fiadoRepository.Registrar(fiado);
    }
}