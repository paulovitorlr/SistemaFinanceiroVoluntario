using CaixaFestejos.Data;
using ClosedXML.Excel;

namespace CaixaFestejos.Repositories;

public class ExportacaoRepository : IExportacaoRepository
{
    private readonly Database _database;

    public ExportacaoRepository(Database database)
    {
        _database = database;
    }

    public void ExportarCsv(string caminho)
    {
        using var conn = _database.AbrirConexao();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
                            SELECT
                            v.DataHora,
                            iv.NomeProduto,
                            iv.Quantidade,
                            iv.PrecoUnit,
                            (iv.PrecoUnit * iv.Quantidade) AS Subtotal,
                            iv.VendaId,
                            v.FormaPagamento
                            FROM ItensVenda iv
                            INNER JOIN Vendas v
                            ON v.Id = iv.VendaId
                            ORDER BY v.DataHora";

        using var reader = cmd.ExecuteReader();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Vendas");

        // Cabeçalho
        worksheet.Cell(1, 1).Value = "Data/Hora";
        worksheet.Cell(1, 2).Value = "Produto";
        worksheet.Cell(1, 3).Value = "Quantidade";
        worksheet.Cell(1, 4).Value = "Preço Unitário";
        worksheet.Cell(1, 5).Value = "Subtotal";
        worksheet.Cell(1, 6).Value = "Venda";
        worksheet.Cell(1, 7).Value = "Forma de Pagamento";

        var header = worksheet.Range(1, 1, 1, 7);

        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int linha = 2;

        while (reader.Read())
        {
            var dataHora = DateTime.Parse(reader.GetString(0));
            var nome = reader.GetString(1);
            var quantidade = reader.GetInt64(2);
            var preco = reader.GetDouble(3);
            var subtotal = reader.GetDouble(4);
            var vendaId = reader.GetInt64(5);
            var formaPagamento = reader.GetString(6);

            worksheet.Cell(linha, 1).Value = dataHora;
            worksheet.Cell(linha, 2).Value = nome;
            worksheet.Cell(linha, 3).Value = quantidade;
            worksheet.Cell(linha, 4).Value = preco;
            worksheet.Cell(linha, 5).Value = subtotal;
            worksheet.Cell(linha, 6).Value = vendaId;
            worksheet.Cell(linha, 7).Value = formaPagamento;

            linha++;
        }

        // Formatação
        worksheet.Column(1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
        worksheet.Column(4).Style.NumberFormat.Format = "R$ #,##0.00";
        worksheet.Column(5).Style.NumberFormat.Format = "R$ #,##0.00";

        // Tabela com filtros
        worksheet.Range(1, 1, linha - 1, 7).CreateTable();

        // Ajuste automático das colunas
        worksheet.Columns().AdjustToContents();

        // Salva como Excel
        workbook.SaveAs(caminho);
    }
}