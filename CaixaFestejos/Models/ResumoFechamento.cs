namespace CaixaFestejos.Models
{
    public class ResumoFechamento
    {
        public decimal TotalVendido { get; set; }
        public int ItensVendidos { get; set; }
        public decimal TotalRecebido { get; set; }
        public decimal TotalTroco { get; set; }
        public decimal Lucro { get; set; }
        public string MaisVendido { get; set; } = "—";
    }

    public class ProdutoVendido
    {
        public string Nome { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal Total { get; set; }
    }
}
