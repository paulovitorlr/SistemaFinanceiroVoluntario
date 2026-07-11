using CaixaFestejos.Repositories;
using CaixaFestejos.Services;

namespace CaixaFestejos.Services;

public class ExportacaoService : IExportacaoService
{
    private readonly IExportacaoRepository _exportacaoRepository;

    public ExportacaoService(IExportacaoRepository exportacaoRepository)
    {
        _exportacaoRepository = exportacaoRepository;
    }

    public void ExportarCsv(string caminho)
    {
        _exportacaoRepository.ExportarCsv(caminho);
    }
}