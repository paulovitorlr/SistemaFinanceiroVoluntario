using CaixaFestejos.Models;

namespace CaixaFestejos.Services.Interfaces;

    public interface IVendaService
    {
        void RegistrarVenda(
            decimal total,
            decimal custoTotal,
            decimal recebido,
            decimal troco,
            List<ItemPedido> itens);

        void ZerarVendas();
    }
