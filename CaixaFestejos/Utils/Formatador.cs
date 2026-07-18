using System.Globalization;

namespace CaixaFestejos.Utils
{
    /// <summary>
    /// Formatação monetária compartilhada entre todas as abas.
    /// </summary>
    public static class Formatador
    {
        public static readonly CultureInfo PtBr = new("pt-BR");

        public static string Moeda(decimal valor) => valor.ToString("C2", PtBr);
    }
}