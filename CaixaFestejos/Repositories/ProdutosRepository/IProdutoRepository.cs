using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories.Interfaces
{
    public interface IProdutoRepository
    {
        List<Produto> Listar();

        Produto? BuscarPorId(int id);

        void Adicionar(Produto produto);

        void Atualizar(Produto produto);

        void Excluir(int id);
    }
}