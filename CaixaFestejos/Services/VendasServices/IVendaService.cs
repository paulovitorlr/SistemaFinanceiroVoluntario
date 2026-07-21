using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

public interface IVendaService
{
    void RegistrarVenda(
        List<ItemPedido> itens,
        decimal recebido,
        FormaPagamento formaPagamento);

    /// <summary>Todas as vendas já finalizadas (sem os itens carregados), mais recentes primeiro.</summary>
    List<Venda> ListarVendas();

    /// <summary>Uma venda finalizada específica, com os itens carregados. Null se o id não existir.</summary>
    Venda? ObterVenda(int vendaId);

    /// <summary>
    /// Substitui os itens de uma venda já finalizada e recalcula Total, CustoTotal e Troco.
    /// Recebido e forma de pagamento da venda original são preservados.
    /// Lança InvalidOperationException se a venda não existir ou ficar sem itens.
    /// </summary>
    void EditarVenda(int vendaId, List<ItemPedido> novosItens);

    ResumoPagamento ObterResumoPagamento();

    void ZerarVendas();
}