using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

    public interface IVendaRepository
    {
        void Registrar(Venda venda);
        Venda? BuscarPorId(int id);
        List<Venda> Listar();
        ResumoPagamento ObterResumoPagamento();
        void Excluir(int id);
        void Zerar();
    }
