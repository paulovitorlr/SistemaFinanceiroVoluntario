using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace CaixaFestejos.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public string CaminhoArquivoBanco { get; }

        public Database()
        {
            string pastaDados = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CaixaFestejos");

            Directory.CreateDirectory(pastaDados);

            CaminhoArquivoBanco = Path.Combine(pastaDados, "caixa.db");

            _connectionString = $"Data Source={CaminhoArquivoBanco}";
        }

        public SqliteConnection AbrirConexao()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}