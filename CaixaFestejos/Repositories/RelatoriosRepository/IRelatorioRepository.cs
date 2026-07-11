using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

    public interface IRelatorioRepository
    {
        ResumoFechamento ObterResumo();

        List<ProdutoVendido> ObterVendasPorProduto();
    }
