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
            FormaPagamento formaPagamento)
        {
            if (itens == null || itens.Count == 0)
                throw new InvalidOperationException("A venda deve possuir pelo menos um item.");

            decimal total = itens.Sum(i => i.Subtotal);

            if (formaPagamento != FormaPagamento.Fiado && recebido < total)
                throw new InvalidOperationException("Valor recebido insuficiente.");

            decimal custoTotal = itens.Sum(i => i.Custo * i.Quantidade);

            decimal troco = recebido - total;


            var venda = new Venda
            {
                DataHora = DateTime.Now,
                Total = total,
                CustoTotal = custoTotal,
                Recebido = recebido,
                Troco = troco,
                FormaPagamento = formaPagamento,
                

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

        public List<Venda> ListarVendas()
        {
            return _vendaRepository.Listar();
        }

        public Venda? ObterVenda(int vendaId)
        {
            return _vendaRepository.BuscarPorId(vendaId);
        }

        public void EditarVenda(int vendaId, List<ItemPedido> novosItens)
        {
            if (novosItens == null || novosItens.Count == 0)
                throw new InvalidOperationException("A venda deve possuir pelo menos um item.");

            var vendaExistente = _vendaRepository.BuscarPorId(vendaId);

            if (vendaExistente == null)
                throw new InvalidOperationException("Venda não encontrada.");

            decimal total = novosItens.Sum(i => i.Subtotal);
            decimal custoTotal = novosItens.Sum(i => i.Custo * i.Quantidade);

            // Recebido e forma de pagamento não mudam aqui: só a composição do pedido está sendo corrigida.
            decimal troco = vendaExistente.FormaPagamento == FormaPagamento.Fiado
                ? 0
                : vendaExistente.Recebido - total;

            if (vendaExistente.FormaPagamento != FormaPagamento.Fiado && vendaExistente.Recebido < total)
                throw new InvalidOperationException("O novo total ficou maior que o valor recebido nessa venda.");

            vendaExistente.Total = total;
            vendaExistente.CustoTotal = custoTotal;
            vendaExistente.Troco = troco;

            vendaExistente.Itens = novosItens.Select(i => new ItemVenda
            {
                VendaId = vendaId,
                ProdutoId = i.ProdutoId,
                Nome = i.Nome,
                Preco = i.Preco,
                Custo = i.Custo,
                Quantidade = i.Quantidade
            }).ToList();

            _vendaRepository.Atualizar(vendaExistente);
        }

        public void ZerarVendas()
        {
            _vendaRepository.Zerar();
        }
    }
}