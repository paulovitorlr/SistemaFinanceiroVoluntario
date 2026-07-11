using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories.Interfaces
{
    public interface IRelatorioRepository
    {
        ResumoFechamento ObterResumo();

        List<ProdutoVendido> ObterVendasPorProduto();
    }
}