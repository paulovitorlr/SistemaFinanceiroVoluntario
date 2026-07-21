using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

/// <summary>
/// Gerencia o pedido atual (em memória) enquanto o usuário monta a venda na aba "Vender".
/// Não grava nada no banco — isso só acontece quando IVendaService.RegistrarVenda
/// (ou IFiadoService.RegistrarFiado) é chamado com os itens já prontos.
/// Não tem qualquer relação com o catálogo/estoque de produtos.
/// </summary>
public interface IPedidoService
{
    /// <summary>Itens do pedido atual, somente leitura (o Form não deve manipular a lista diretamente).</summary>
    IReadOnlyList<ItemPedido> Itens { get; }

    /// <summary>Adiciona um produto ao pedido. Se o produto já estiver no pedido, soma 1 à quantidade existente.</summary>
    void AdicionarItem(Produto produto);

    /// <summary>
    /// Define a quantidade exata de um item já presente no pedido.
    /// Se novaQuantidade for menor ou igual a zero, o item é removido do pedido.
    /// Lança InvalidOperationException se o produto não estiver no pedido atual.
    /// </summary>
    void AlterarQuantidadeItem(int produtoId, int novaQuantidade);

    /// <summary>Remove completamente um item do pedido atual, independente da quantidade.</summary>
    void RemoverItemPedido(int produtoId);

    /// <summary>
    /// Substitui todo o conteúdo do pedido atual pelos itens informados (cópias independentes).
    /// Usado para carregar, em memória, os itens de uma venda já finalizada que está sendo corrigida.
    /// </summary>
    void CarregarItens(IEnumerable<ItemPedido> itens);

    /// <summary>Soma dos subtotais (preço x quantidade) de todos os itens do pedido atual.</summary>
    decimal RecalcularPedido();

    /// <summary>Soma do custo (custo x quantidade) de todos os itens do pedido atual.</summary>
    decimal RecalcularCustoTotal();

    /// <summary>Lucro estimado do pedido atual (total de venda menos custo total).</summary>
    decimal RecalcularLucro();

    /// <summary>Esvazia o pedido atual. Deve ser chamado após a venda/fiado ser finalizado com sucesso.</summary>
    void LimparPedido();
}
