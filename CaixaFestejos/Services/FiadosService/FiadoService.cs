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

    public List<Fiado> ListarFiados()
    {
        return _fiadoRepository.Listar();
    }

    public List<Fiado> ListarFiadosPendentes()
    {
        return _fiadoRepository.ListarPendentes();
    }

    public List<Fiado> ListarFiadosPagos()
    {
        return _fiadoRepository.ListarQuitados();
    }

    public decimal ObterTotalPendente()
    {
        // Sempre soma o saldo ATUAL dos fiados em aberto (Pago = 0),
        // nunca um histórico por dia. Por isso uma dívida de dias anteriores
        // nunca é contada de novo: ela só some da lista quando é quitada.
        return _fiadoRepository.ListarPendentes().Sum(f => f.ValorTotal);
    }

    public void ReceberPagamento(int fiadoId, decimal valorRecebido, FormaPagamento formaPagamento)
    {
        var fiado = _fiadoRepository.BuscarPorId(fiadoId);

        if (fiado is null)
            throw new InvalidOperationException("Fiado não encontrado.");

        if (fiado.Pago)
            throw new InvalidOperationException("Este fiado já foi quitado.");

        if (valorRecebido <= 0)
            throw new InvalidOperationException("Informe um valor de recebimento válido.");

        if (valorRecebido < fiado.ValorTotal)
            throw new InvalidOperationException(
                $"O valor recebido (R$ {valorRecebido:0.00}) é menor que o valor devido (R$ {fiado.ValorTotal:0.00}).");

        _fiadoRepository.Quitar(fiadoId, formaPagamento);
    }

    public void ExcluirFiado(int fiadoId)
    {
        var fiado = _fiadoRepository.BuscarPorId(fiadoId);

        if (fiado is null)
            throw new InvalidOperationException("Fiado não encontrado.");

        _fiadoRepository.Excluir(fiadoId);
    }
}