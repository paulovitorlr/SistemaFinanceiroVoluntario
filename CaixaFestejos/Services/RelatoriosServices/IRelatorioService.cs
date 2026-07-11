using CaixaFestejos.Models;

namespace CaixaFestejos.Services;

public interface IRelatorioService
{
    ResumoFechamento ObterResumo();
    List<ProdutoVendido> ObterVendasPorProduto();
}