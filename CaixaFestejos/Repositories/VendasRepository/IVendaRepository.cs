using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

    public interface IVendaRepository
    {
        void Registrar(Venda venda);

        Venda? BuscarPorId(int id);

        List<Venda> Listar();

        void Excluir(int id);

        void Zerar();
    }
