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
        throw new NotImplementedException();
    }

    public List<Fiado> Listar()
    {
        throw new NotImplementedException();
    }

    public List<Fiado> ListarPendentes()
    {
        throw new NotImplementedException();
    }

    public List<Fiado> ListarQuitados()
    {
        throw new NotImplementedException();
    }

    public void Excluir(int id)
    {
        throw new NotImplementedException();
    }

    public void Zerar()
    {
        throw new NotImplementedException();
    }
    public void Quitar(int id, FormaPagamento formaPagamento)
    {
        throw new NotImplementedException();
    }
}