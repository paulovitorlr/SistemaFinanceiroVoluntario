using CaixaFestejos.Data;
using CaixaFestejos.Models;
using CaixaFestejos.Repositories;

using Microsoft.Data.Sqlite;

namespace CaixaFestejos.Repositories;

    public class ProdutoRepository : IProdutoRepository
    {
        private readonly Database _database;

        public ProdutoRepository(Database database)
        {
            _database = database;
        }

        public List<Produto> Listar()
        {
            var lista = new List<Produto>();

            using var conn = _database.AbrirConexao();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"SELECT Id,
                 Nome,
                 Preco,
                 Custo
                 FROM Produtos
                 ORDER BY Nome";

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

        public Produto? BuscarPorId(int id)
        {
            using var conn = _database.AbrirConexao();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"SELECT Id,
                 Nome,
                 Preco,
                 Custo
                 FROM Produtos
                 WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Produto
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Preco = (decimal)reader.GetDouble(2),
                Custo = (decimal)reader.GetDouble(3)
            };
        }

        public void Adicionar(Produto produto)
        {
            using var conn = _database.AbrirConexao();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"INSERT INTO Produtos
                (Nome, Preco, Custo)
                VALUES
                ($nome, $preco, $custo)";

            cmd.Parameters.AddWithValue("$nome", produto.Nome);
            cmd.Parameters.AddWithValue("$preco", (double)produto.Preco);
            cmd.Parameters.AddWithValue("$custo", (double)produto.Custo);

            cmd.ExecuteNonQuery();
        }

        public void Atualizar(Produto produto)
        {
            using var conn = _database.AbrirConexao();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"UPDATE Produtos
                SET Nome = $nome,
                Preco = $preco,
                Custo = $custo
                WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", produto.Id);
            cmd.Parameters.AddWithValue("$nome", produto.Nome);
            cmd.Parameters.AddWithValue("$preco", (double)produto.Preco);
            cmd.Parameters.AddWithValue("$custo", (double)produto.Custo);

            cmd.ExecuteNonQuery();
        }

        public void Excluir(int id)
        {
            using var conn = _database.AbrirConexao();

            using var cmd = conn.CreateCommand();

            cmd.CommandText =
                @"DELETE FROM Produtos
                WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", id);

            cmd.ExecuteNonQuery();
        }
    }
