using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

public class PedidoService : IPedidoService
{
    private readonly List<ItemPedido> _itens = new();

    public IReadOnlyList<ItemPedido> Itens => _itens;

    public void AdicionarItem(Produto produto)
    {
        if (produto == null)
            throw new ArgumentNullException(nameof(produto));

        var item = _itens.FirstOrDefault(i => i.ProdutoId == produto.Id);

        if (item != null)
        {
            item.Quantidade++;
            return;
        }

        _itens.Add(new ItemPedido
        {
            ProdutoId = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Custo = produto.Custo,
            Quantidade = 1
        });
    }

    public void AlterarQuantidadeItem(int produtoId, int novaQuantidade)
    {
        var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);

        if (item == null)
            throw new InvalidOperationException("Item não encontrado no pedido atual.");

        if (novaQuantidade <= 0)
        {
            _itens.Remove(item);
            return;
        }

        item.Quantidade = novaQuantidade;
    }

    public void RemoverItemPedido(int produtoId)
    {
        var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);

        if (item != null)
            _itens.Remove(item);
    }

    public void CarregarItens(IEnumerable<ItemPedido> itens)
    {
        _itens.Clear();

        foreach (var item in itens)
        {
            _itens.Add(new ItemPedido
            {
                ProdutoId = item.ProdutoId,
                Nome = item.Nome,
                Preco = item.Preco,
                Custo = item.Custo,
                Quantidade = item.Quantidade
            });
        }
    }

    public decimal RecalcularPedido()
    {
        return _itens.Sum(i => i.Subtotal);
    }

    public decimal RecalcularCustoTotal()
    {
        return _itens.Sum(i => i.Custo * i.Quantidade);
    }

    public decimal RecalcularLucro()
    {
        return RecalcularPedido() - RecalcularCustoTotal();
    }

    public void LimparPedido()
    {
        _itens.Clear();
    }
}
