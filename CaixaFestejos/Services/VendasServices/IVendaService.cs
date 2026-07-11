using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

    public interface IVendaService
    {
        void RegistrarVenda(List<ItemPedido> itens, decimal recebido);

        void ZerarVendas();
    }
