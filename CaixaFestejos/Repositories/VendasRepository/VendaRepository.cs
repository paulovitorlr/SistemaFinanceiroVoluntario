using CaixaFestejos.Data;
using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly Database _database;

    public VendaRepository(Database database)
    {
        _database = database;
    }

    public void Registrar(Venda venda)
    {
        using var conn = _database.AbrirConexao();
        using var transaction = conn.BeginTransaction();

        long vendaId;

        using (var cmdVenda = conn.CreateCommand())
        {
            cmdVenda.Transaction = transaction;

            cmdVenda.CommandText = @"
            INSERT INTO Vendas
(
    DataHora,
    Total,
    CustoTotal,
    Recebido,
    Troco,
    FormaPagamento
)
VALUES
(
    $dataHora,
    $total,
    $custoTotal,
    $recebido,
    $troco,
    $formaPagamento
);

            SELECT last_insert_rowid();";

            cmdVenda.Parameters.AddWithValue("$dataHora", venda.DataHora.ToString("o"));
            cmdVenda.Parameters.AddWithValue("$total", (double)venda.Total);
            cmdVenda.Parameters.AddWithValue("$custoTotal", (double)venda.CustoTotal);
            cmdVenda.Parameters.AddWithValue("$recebido", (double)venda.Recebido);
            cmdVenda.Parameters.AddWithValue("$troco", (double)venda.Troco);
            cmdVenda.Parameters.AddWithValue("$formaPagamento", venda.FormaPagamento.ToString());

            

            vendaId = (long)cmdVenda.ExecuteScalar()!;
            venda.Id = (int)vendaId;
        }


        foreach (var item in venda.Itens)
        {
            using var cmdItem = conn.CreateCommand();

            cmdItem.Transaction = transaction;

            cmdItem.CommandText = @"
            INSERT INTO ItensVenda
            (
                VendaId,
                ProdutoId,
                NomeProduto,
                PrecoUnit,
                CustoUnit,
                Quantidade
            )
            VALUES
            (
                $vendaId,
                $produtoId,
                $nome,
                $preco,
                $custo,
                $quantidade
            )";

            cmdItem.Parameters.AddWithValue("$vendaId", vendaId);
            cmdItem.Parameters.AddWithValue("$produtoId", item.ProdutoId);
            cmdItem.Parameters.AddWithValue("$nome", item.Nome);
            cmdItem.Parameters.AddWithValue("$preco", (double)item.Preco);
            cmdItem.Parameters.AddWithValue("$custo", (double)item.Custo);
            cmdItem.Parameters.AddWithValue("$quantidade", item.Quantidade);

            cmdItem.ExecuteNonQuery();
        }


        
        


        transaction.Commit();
    }
    public Venda? BuscarPorId(int id)
    {
        using var conn = _database.AbrirConexao();

        Venda? venda = null;

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    Id,
                    DataHora,
                    Total,
                    CustoTotal,
                    Recebido,
                    Troco,
                    FormaPagamento
                FROM Vendas
                WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            venda = new Venda
            {
                Id = reader.GetInt32(0),
                DataHora = DateTime.Parse(reader.GetString(1)),
                Total = (decimal)reader.GetDouble(2),
                CustoTotal = (decimal)reader.GetDouble(3),
                Recebido = (decimal)reader.GetDouble(4),
                Troco = (decimal)reader.GetDouble(5),
                FormaPagamento = Enum.Parse<FormaPagamento>(reader.GetString(6)),
                Itens = new List<ItemVenda>()
            };
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    Id,
                    VendaId,
                    ProdutoId,
                    NomeProduto,
                    PrecoUnit,
                    CustoUnit,
                    Quantidade
                FROM ItensVenda
                WHERE VendaId = $id";

            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                venda!.Itens.Add(new ItemVenda
                {
                    Id = reader.GetInt32(0),
                    VendaId = reader.GetInt32(1),
                    ProdutoId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Nome = reader.GetString(3),
                    Preco = (decimal)reader.GetDouble(4),
                    Custo = (decimal)reader.GetDouble(5),
                    Quantidade = reader.GetInt32(6)
                });
            }
        }

        return venda;
    }

    public List<Venda> Listar()
    {
        var vendas = new List<Venda>();

        using var conn = _database.AbrirConexao();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT
                Id,
                DataHora,
                Total,
                CustoTotal,
                Recebido,
                Troco,
                FormaPagamento
            FROM Vendas
            ORDER BY DataHora DESC";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            vendas.Add(new Venda
            {
                Id = reader.GetInt32(0),
                DataHora = DateTime.Parse(reader.GetString(1)),
                Total = (decimal)reader.GetDouble(2),
                CustoTotal = (decimal)reader.GetDouble(3),
                Recebido = (decimal)reader.GetDouble(4),
                Troco = (decimal)reader.GetDouble(5),
                FormaPagamento = Enum.Parse<FormaPagamento>(reader.GetString(6))
            });
        }

        return vendas;
    }

    public ResumoPagamento ObterResumoPagamento()
    {
        using var conn = _database.AbrirConexao();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
        SELECT
            COALESCE(SUM(Total), 0) AS TotalGeral,

            COALESCE(SUM(CASE
                WHEN FormaPagamento = 'Especie'
                THEN Total
                ELSE 0
            END), 0) AS TotalEspecie,

            COALESCE(SUM(CASE
                WHEN FormaPagamento = 'Pix'
                THEN Total
                ELSE 0
            END), 0) AS TotalPix

            FROM Vendas;";

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
        {
            return new ResumoPagamento();
        }

        return new ResumoPagamento
        {
            TotalGeral = (decimal)reader.GetDouble(0),
            TotalEspecie = (decimal)reader.GetDouble(1),
            TotalPix = (decimal)reader.GetDouble(2)
        };
    }
    public void Excluir(int id)
    {
        using var conn = _database.AbrirConexao();
        using var transaction = conn.BeginTransaction();

        

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM ItensVenda WHERE VendaId = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM Vendas WHERE Id = $id";
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
            cmd.CommandText = "DELETE FROM ItensVenda";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = "DELETE FROM Vendas";
            cmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}