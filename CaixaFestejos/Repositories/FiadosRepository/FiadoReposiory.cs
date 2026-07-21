using CaixaFestejos.Data;
using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

public class FiadoRepository : IFiadoRepository
{
    private readonly Database _database;

    public FiadoRepository(Database database)
    {
        _database = database;
    }

    public void Registrar(Fiado fiado)
    {
        using var conn = _database.AbrirConexao();
        using var transaction = conn.BeginTransaction();

        long fiadoId;

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;

            cmd.CommandText = @"
                INSERT INTO Fiados
                (
                    Cliente,
                    DataCriacao,
                    ValorTotal,
                    Pago,
                    DataPagamento
                )
                VALUES
                (
                    $cliente,
                    $dataCriacao,
                    $valorTotal,
                    $pago,
                    $dataPagamento
                );

                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$cliente", fiado.Cliente);
            cmd.Parameters.AddWithValue("$dataCriacao", fiado.DataCriacao.ToString("o"));
            cmd.Parameters.AddWithValue("$valorTotal", (double)fiado.ValorTotal);
            cmd.Parameters.AddWithValue("$pago", fiado.Pago ? 1 : 0);
            cmd.Parameters.AddWithValue(
                "$dataPagamento",
                fiado.DataPagamento?.ToString("o") ?? (object)DBNull.Value
            );

            fiadoId = (long)cmd.ExecuteScalar()!;
            fiado.Id = (int)fiadoId;
        }

        foreach (var item in fiado.Itens)
        {
            using var cmd = conn.CreateCommand();

            cmd.Transaction = transaction;

            cmd.CommandText = @"
                INSERT INTO ItensFiado
                (
                    FiadoId,
                    ProdutoId,
                    NomeProduto,
                    PrecoUnit,
                    CustoUnit,
                    Quantidade
                )
                VALUES
                (
                    $fiadoId,
                    $produtoId,
                    $nome,
                    $preco,
                    $custo,
                    $quantidade
                );";

            cmd.Parameters.AddWithValue("$fiadoId", fiadoId);
            cmd.Parameters.AddWithValue("$produtoId", item.ProdutoId);
            cmd.Parameters.AddWithValue("$nome", item.Nome);
            cmd.Parameters.AddWithValue("$preco", (double)item.PrecoUnitario);
            cmd.Parameters.AddWithValue("$custo", (double)item.CustoUnitario);
            cmd.Parameters.AddWithValue("$quantidade", item.Quantidade);

            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public Fiado? BuscarPorId(int id)
    {
        using var conn = _database.AbrirConexao();

        Fiado? fiado;

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    Id,
                    Cliente,
                    DataCriacao,
                    ValorTotal,
                    Pago,
                    DataPagamento,
                    FormaPagamento
                FROM Fiados
                WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            fiado = MapearFiado(reader);
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    ProdutoId,
                    NomeProduto,
                    PrecoUnit,
                    CustoUnit,
                    Quantidade
                FROM ItensFiado
                WHERE FiadoId = $id";

            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                fiado!.Itens.Add(MapearItem(reader));
            }
        }

        return fiado;
    }

    public List<Fiado> Listar()
    {
        return ListarComFiltro(null);
    }

    public List<Fiado> ListarPendentes()
    {
        return ListarComFiltro(pago: false);
    }

    public List<Fiado> ListarQuitados()
    {
        return ListarComFiltro(pago: true);
    }

    private List<Fiado> ListarComFiltro(bool? pago)
    {
        var fiados = new List<Fiado>();

        using var conn = _database.AbrirConexao();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                Id,
                Cliente,
                DataCriacao,
                ValorTotal,
                Pago,
                DataPagamento,
                FormaPagamento
            FROM Fiados
            " + (pago is null ? "" : "WHERE Pago = $pago") + @"
            ORDER BY DataCriacao DESC";

        if (pago is not null)
            cmd.Parameters.AddWithValue("$pago", pago.Value ? 1 : 0);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            fiados.Add(MapearFiado(reader));
        }

        return fiados;
    }

    public void Quitar(int id, FormaPagamento formaPagamento)
    {
        using var conn = _database.AbrirConexao();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            UPDATE Fiados
            SET
                Pago = 1,
                DataPagamento = $dataPagamento,
                FormaPagamento = $formaPagamento
            WHERE Id = $id";

        cmd.Parameters.AddWithValue("$dataPagamento", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$formaPagamento", formaPagamento.ToString());
        cmd.Parameters.AddWithValue("$id", id);

        cmd.ExecuteNonQuery();
    }

    public void Excluir(int id)
    {
        using var conn = _database.AbrirConexao();
        using var transaction = conn.BeginTransaction();

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM ItensFiado WHERE FiadoId = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM Fiados WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void Zerar()
    {
        using var conn = _database.AbrirConexao();
        using var transaction = conn.BeginTransaction();

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM ItensFiado";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM Fiados";
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static Fiado MapearFiado(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new Fiado
        {
            Id = reader.GetInt32(0),
            Cliente = reader.GetString(1),
            DataCriacao = DateTime.Parse(reader.GetString(2)),
            ValorTotal = (decimal)reader.GetDouble(3),
            Pago = reader.GetInt32(4) == 1,
            DataPagamento = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            FormaPagamentoRecebimento = reader.IsDBNull(6)
                ? null
                : Enum.Parse<FormaPagamento>(reader.GetString(6)),
            Itens = new List<ItemFiado>()
        };
    }

    private static ItemFiado MapearItem(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new ItemFiado
        {
            ProdutoId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
            Nome = reader.GetString(1),
            PrecoUnitario = (decimal)reader.GetDouble(2),
            CustoUnitario = (decimal)reader.GetDouble(3),
            Quantidade = reader.GetInt32(4)
        };
    }
}