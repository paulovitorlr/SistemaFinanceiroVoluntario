using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

public interface IFiadoService
{
    void RegistrarFiado(
        string cliente,
        List<ItemPedido> pedido);

    /// <summary>Todos os fiados, mais recentes primeiro.</summary>
    List<Fiado> ListarFiados();

    /// <summary>Fiados ainda em aberto (não pagos).</summary>
    List<Fiado> ListarFiadosPendentes();

    /// <summary>Fiados já quitados.</summary>
    List<Fiado> ListarFiadosPagos();

    /// <summary>Soma do valor de todos os fiados em aberto (usado no fechamento, sem duplicar dívidas de dias anteriores).</summary>
    decimal ObterTotalPendente();

    /// <summary>
    /// Confirma o recebimento de um fiado: valida se ele existe, se já não foi pago
    /// e se o valor confere, e então marca como quitado.
    /// </summary>
    void ReceberPagamento(int fiadoId, decimal valorRecebido, FormaPagamento formaPagamento);

    void ExcluirFiado(int fiadoId);
}