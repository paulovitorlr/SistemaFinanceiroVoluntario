using CaixaFestejos.Models;
using CaixaFestejos.Repositories;
using CaixaFestejos.Services;

namespace CaixaFestejos.Services
{
    public class VendaService : IVendaService
    {
        private readonly IVendaRepository _vendaRepository;

        public VendaService(IVendaRepository vendaRepository)
        {
            _vendaRepository = vendaRepository;
        }

        public void RegistrarVenda(
     List<ItemPedido> itens,
     decimal recebido,
     FormaPagamento formaPagamento,
     string? clienteFiado)
        {
            if (itens == null || itens.Count == 0)
                throw new InvalidOperationException("A venda deve possuir pelo menos um item.");

            decimal total = itens.Sum(i => i.Subtotal);

            if (formaPagamento != FormaPagamento.Fiado && recebido < total)
                throw new InvalidOperationException("Valor recebido insuficiente.");

            if (formaPagamento == FormaPagamento.Fiado &&
                string.IsNullOrWhiteSpace(clienteFiado))
            {
                throw new InvalidOperationException(
                    "Informe o nome do cliente para venda fiada.");
            }

            decimal custoTotal = itens.Sum(i => i.Custo * i.Quantidade);

            decimal troco = formaPagamento == FormaPagamento.Fiado
                ? 0
                : recebido - total;


            var venda = new Venda
            {
                DataHora = DateTime.Now,
                Total = total,
                CustoTotal = custoTotal,
                Recebido = recebido,
                Troco = troco,
                FormaPagamento = formaPagamento,
                ClienteFiado = clienteFiado,

                Itens = itens.Select(i => new ItemVenda
                {
                    ProdutoId = i.ProdutoId,
                    Nome = i.Nome,
                    Preco = i.Preco,
                    Custo = i.Custo,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            _vendaRepository.Registrar(venda);
        }

        public ResumoPagamento ObterResumoPagamento()
        {
            return _vendaRepository.ObterResumoPagamento();
        }

        public void ZerarVendas()
        {
            _vendaRepository.Zerar();
        }
    }
}