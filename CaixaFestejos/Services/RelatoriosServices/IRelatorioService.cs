using CaixaFestejos.Models;

namespace CaixaFestejos.Services.Interfaces;

    public interface IRelatorioService
    {
        ResumoFechamento ObterResumo();
        List<ProdutoVendido> ObterVendasPorProduto();
        void ExportarCsv(string caminho);
    }
