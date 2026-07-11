using CaixaFestejos.Models;
using CaixaFestejos.Repositories;
using CaixaFestejos.Services;

namespace CaixaFestejos.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IRelatorioRepository _relatorioRepository;

    public RelatorioService(IRelatorioRepository relatorioRepository)
    {
        _relatorioRepository = relatorioRepository;
    }

    public ResumoFechamento ObterResumo()
    {
        return _relatorioRepository.ObterResumo();
    }

    public List<ProdutoVendido> ObterVendasPorProduto()
    {
        return _relatorioRepository.ObterVendasPorProduto();
    }
}