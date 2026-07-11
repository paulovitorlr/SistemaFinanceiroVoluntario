using CaixaFestejos.Models;

namespace CaixaFestejos.Services.Interfaces;

    public interface IProdutoService
    {
        List<Produto> ListarProdutos();
        void AdicionarProduto(Produto produto);
        void ExcluirProduto(int id);
    }
