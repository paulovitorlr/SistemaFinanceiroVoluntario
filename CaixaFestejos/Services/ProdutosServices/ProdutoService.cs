using CaixaFestejos.Models;
using CaixaFestejos.Repositories;
using CaixaFestejos.Services.Interfaces;

namespace CaixaFestejos.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoService(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public List<Produto> ListarProdutos()
        {
            return _produtoRepository.Listar();
        }

        public void AdicionarProduto(Produto produto)
        {
            _produtoRepository.Adicionar(produto);
        }

        public void ExcluirProduto(int id)
        {
            _produtoRepository.Excluir(id);
        }
    }
}