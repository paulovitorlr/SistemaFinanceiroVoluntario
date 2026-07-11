namespace CaixaFestejos.Models
{
    public abstract class Item
    {
        public int ProdutoId { get; set; }

        public string Nome { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public decimal Custo { get; set; }

        public int Quantidade { get; set; }

        public decimal Subtotal => Preco * Quantidade;
    }
}