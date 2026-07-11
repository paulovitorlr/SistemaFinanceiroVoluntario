namespace CaixaFestejos.Repositories;

public interface IExportacaoRepository
{
    void ExportarCsv(string caminho);
}