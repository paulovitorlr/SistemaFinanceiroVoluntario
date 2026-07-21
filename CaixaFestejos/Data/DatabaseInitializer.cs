using Microsoft.Data.Sqlite;

namespace CaixaFestejos.Data
{
    public class DatabaseInitializer
    {
        private readonly Database _database;

        public DatabaseInitializer(Database database)
        {
            _database = database;
        }

        public void Inicializar()
        {
            using var conn = _database.AbrirConexao();
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
                    Troco REAL NOT NULL,
                    FormaPagamento TEXT NOT NULL DEFAULT 'Especie'
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
                );

                CREATE TABLE IF NOT EXISTS Fiados (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Cliente TEXT NOT NULL,
                    DataCriacao TEXT NOT NULL,
                    ValorTotal REAL NOT NULL,
                    Pago INTEGER NOT NULL DEFAULT 0,
                    DataPagamento TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS ItensFiado (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FiadoId INTEGER NOT NULL,
                    ProdutoId INTEGER,
                    NomeProduto TEXT NOT NULL,
                    PrecoUnit REAL NOT NULL,
                    CustoUnit REAL NOT NULL,
                    Quantidade INTEGER NOT NULL,
                    FOREIGN KEY (FiadoId) REFERENCES Fiados(Id)
                );
            ";

            cmd.ExecuteNonQuery();
        }
    }
}