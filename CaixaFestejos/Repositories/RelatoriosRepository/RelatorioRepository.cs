using CaixaFestejos.Data;
using CaixaFestejos.Models;
using CaixaFestejos.Repositories.Interfaces;

namespace CaixaFestejos.Repositories;

public class RelatorioRepository : IRelatorioRepository
{
    private readonly Database _database;

    public RelatorioRepository(Database database)
    {
        _database = database;
    }

    public ResumoFechamento ObterResumo()
    {
        using var conn = _database.AbrirConexao();

        var resumo = new ResumoFechamento();

        // Totais gerais
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                IFNULL(SUM(Total), 0),
                IFNULL(SUM(Recebido), 0),
                IFNULL(SUM(Troco), 0),
                IFNULL(SUM(CustoTotal), 0)
                FROM Vendas";

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                resumo.TotalVendido = (decimal)reader.GetDouble(0);
                resumo.TotalRecebido = (decimal)reader.GetDouble(1);
                resumo.TotalTroco = (decimal)reader.GetDouble(2);

                var custo = (decimal)reader.GetDouble(3);

                resumo.Lucro = resumo.TotalVendido - custo;
            }
        }

        
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
            SELECT IFNULL(SUM(Quantidade), 0)
            FROM ItensVenda";

            resumo.ItensVendidos = Convert.ToInt32(cmd.ExecuteScalar());
        }

        
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
            SELECT NomeProduto
            FROM ItensVenda
            GROUP BY NomeProduto
            ORDER BY SUM(Quantidade) DESC
            LIMIT 1";

            resumo.MaisVendido = cmd.ExecuteScalar()?.ToString() ?? "—";
        }

        return resumo;
    }

    public List<ProdutoVendido> ObterVendasPorProduto()
    {
        var lista = new List<ProdutoVendido>();

        using var conn = _database.AbrirConexao();

        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
            NomeProduto,
            SUM(Quantidade),
            SUM(PrecoUnit * Quantidade)
            FROM ItensVenda
            GROUP BY NomeProduto
            ORDER BY NomeProduto";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new ProdutoVendido
            {
                Nome = reader.GetString(0),
                Quantidade = reader.GetInt32(1),
                Total = (decimal)reader.GetDouble(2)
            });
        }

        return lista;
    }
}