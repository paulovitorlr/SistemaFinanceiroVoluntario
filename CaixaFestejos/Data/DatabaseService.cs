using Microsoft.Data.Sqlite;
using CaixaFestejos.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace CaixaFestejos.Data
{
    // Toda a persistência do sistema. O banco fica em um arquivo .db local,
    // dentro da pasta AppData do usuário do Windows, então funciona sem
    // internet e sem instalar nenhum servidor de banco de dados.
    public class DatabaseService
    {
        private readonly string _connectionString;
        public string CaminhoArquivoBanco { get; }

        public DatabaseService()
        {
            string pastaDados = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CaixaFestejos");
            Directory.CreateDirectory(pastaDados);

            CaminhoArquivoBanco = Path.Combine(pastaDados, "caixa.db");
            _connectionString = $"Data Source={CaminhoArquivoBanco}";
            CriarTabelas();
        }

        private SqliteConnection AbrirConexao()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private void CriarTabelas()
        {
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Produtos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Preco REAL NOT NULL,
                    Custo REAL NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Vendas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DataHora TEXT NOT NULL,
                    Total REAL NOT NULL,
                    CustoTotal REAL NOT NULL,
                    Recebido REAL NOT NULL,
                    Troco REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ItensVenda (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VendaId INTEGER NOT NULL,
                    ProdutoId INTEGER,
                    NomeProduto TEXT NOT NULL,
                    PrecoUnit REAL NOT NULL,
                    CustoUnit REAL NOT NULL,
                    Quantidade INTEGER NOT NULL,
                    FOREIGN KEY (VendaId) REFERENCES Vendas(Id)
                );";
            cmd.ExecuteNonQuery();
        }

        // ---------- Produtos ----------

        public List<Produto> ListarProdutos()
        {
            var lista = new List<Produto>();
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Nome, Preco, Custo FROM Produtos ORDER BY Nome";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Produto
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Preco = (decimal)reader.GetDouble(2),
                    Custo = (decimal)reader.GetDouble(3)
                });
            }
            return lista;
        }

        public void AdicionarProduto(Produto produto)
        {
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Produtos (Nome, Preco, Custo) VALUES ($nome, $preco, $custo)";
            cmd.Parameters.AddWithValue("$nome", produto.Nome);
            cmd.Parameters.AddWithValue("$preco", (double)produto.Preco);
            cmd.Parameters.AddWithValue("$custo", (double)produto.Custo);
            cmd.ExecuteNonQuery();
        }

        public void ExcluirProduto(int id)
        {
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Produtos WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ---------- Vendas ----------

        public void RegistrarVenda(decimal total, decimal custoTotal, decimal recebido, decimal troco, List<ItemPedido> itens)
        {
            using var conn = AbrirConexao();
            using var transacao = conn.BeginTransaction();

            long vendaId;
            using (var cmdVenda = conn.CreateCommand())
            {
                cmdVenda.Transaction = transacao;
                cmdVenda.CommandText = @"
                    INSERT INTO Vendas (DataHora, Total, CustoTotal, Recebido, Troco)
                    VALUES ($dataHora, $total, $custoTotal, $recebido, $troco);
                    SELECT last_insert_rowid();";
                cmdVenda.Parameters.AddWithValue("$dataHora", DateTime.Now.ToString("o"));
                cmdVenda.Parameters.AddWithValue("$total", (double)total);
                cmdVenda.Parameters.AddWithValue("$custoTotal", (double)custoTotal);
                cmdVenda.Parameters.AddWithValue("$recebido", (double)recebido);
                cmdVenda.Parameters.AddWithValue("$troco", (double)troco);
                vendaId = (long)cmdVenda.ExecuteScalar()!;
            }

            foreach (var item in itens)
            {
                using var cmdItem = conn.CreateCommand();
                cmdItem.Transaction = transacao;
                cmdItem.CommandText = @"
                    INSERT INTO ItensVenda (VendaId, ProdutoId, NomeProduto, PrecoUnit, CustoUnit, Quantidade)
                    VALUES ($vendaId, $produtoId, $nome, $preco, $custo, $qtd)";
                cmdItem.Parameters.AddWithValue("$vendaId", vendaId);
                cmdItem.Parameters.AddWithValue("$produtoId", item.ProdutoId);
                cmdItem.Parameters.AddWithValue("$nome", item.Nome);
                cmdItem.Parameters.AddWithValue("$preco", (double)item.Preco);
                cmdItem.Parameters.AddWithValue("$custo", (double)item.Custo);
                cmdItem.Parameters.AddWithValue("$qtd", item.Quantidade);
                cmdItem.ExecuteNonQuery();
            }

            transacao.Commit();
        }

        public ResumoFechamento ObterResumo()
        {
            var resumo = new ResumoFechamento();
            using var conn = AbrirConexao();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COALESCE(SUM(Total),0), COALESCE(SUM(Recebido),0),
                           COALESCE(SUM(Troco),0), COALESCE(SUM(CustoTotal),0)
                    FROM Vendas";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    resumo.TotalVendido = (decimal)reader.GetDouble(0);
                    resumo.TotalRecebido = (decimal)reader.GetDouble(1);
                    resumo.TotalTroco = (decimal)reader.GetDouble(2);
                    decimal custo = (decimal)reader.GetDouble(3);
                    resumo.Lucro = resumo.TotalVendido - custo;
                }
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COALESCE(SUM(Quantidade),0) FROM ItensVenda";
                resumo.ItensVendidos = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT NomeProduto, SUM(Quantidade) as Qtd
                    FROM ItensVenda GROUP BY NomeProduto
                    ORDER BY Qtd DESC LIMIT 1";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    resumo.MaisVendido = $"{reader.GetString(0)} ({reader.GetInt64(1)})";
                }
            }

            return resumo;
        }

        public List<ProdutoVendido> ObterVendasPorProduto()
        {
            var lista = new List<ProdutoVendido>();
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT NomeProduto, SUM(Quantidade), SUM(PrecoUnit * Quantidade)
                FROM ItensVenda
                GROUP BY NomeProduto
                ORDER BY SUM(Quantidade) DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new ProdutoVendido
                {
                    Nome = reader.GetString(0),
                    Quantidade = Convert.ToInt32(reader.GetInt64(1)),
                    Total = (decimal)reader.GetDouble(2)
                });
            }
            return lista;
        }

        public void ZerarVendas()
        {
            using var conn = AbrirConexao();
            using var transacao = conn.BeginTransaction();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = transacao;
                cmd.CommandText = "DELETE FROM ItensVenda";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = transacao;
                cmd.CommandText = "DELETE FROM Vendas";
                cmd.ExecuteNonQuery();
            }

            transacao.Commit();
        }

        public void ExportarCsv(string caminho)
        {
            using var conn = AbrirConexao();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT v.DataHora, iv.NomeProduto, iv.Quantidade, iv.PrecoUnit,
                       (iv.PrecoUnit * iv.Quantidade) as Subtotal, iv.VendaId
                FROM ItensVenda iv
                JOIN Vendas v ON v.Id = iv.VendaId
                ORDER BY v.DataHora";
            using var reader = cmd.ExecuteReader();

            using var writer = new StreamWriter(caminho, false, System.Text.Encoding.UTF8);
            writer.WriteLine("data_hora;produto;quantidade;preco_unit;subtotal;venda_id");
            while (reader.Read())
            {
                string dataHora = reader.GetString(0);
                string nome = reader.GetString(1);
                long qtd = reader.GetInt64(2);
                double preco = reader.GetDouble(3);
                double subtotal = reader.GetDouble(4);
                long vendaId = reader.GetInt64(5);
                writer.WriteLine($"{dataHora};{nome};{qtd};{preco:F2};{subtotal:F2};{vendaId}");
            }
        }
    }
}
