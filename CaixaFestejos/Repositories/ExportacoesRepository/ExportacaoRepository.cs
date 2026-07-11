using CaixaFestejos.Data;

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
                iv.VendaId
            FROM ItensVenda iv
            INNER JOIN Vendas v
                ON v.Id = iv.VendaId
            ORDER BY v.DataHora";

        using var reader = cmd.ExecuteReader();

        using var writer = new StreamWriter(
            caminho,
            false,
            System.Text.Encoding.UTF8);

        writer.WriteLine("data_hora;produto;quantidade;preco_unit;subtotal;venda_id");

        while (reader.Read())
        {
            string dataHora = reader.GetString(0);
            string nome = reader.GetString(1);
            long quantidade = reader.GetInt64(2);
            double preco = reader.GetDouble(3);
            double subtotal = reader.GetDouble(4);
            long vendaId = reader.GetInt64(5);

            writer.WriteLine(
                $"{dataHora};{nome};{quantidade};{preco:F2};{subtotal:F2};{vendaId}");
        }
    }
}