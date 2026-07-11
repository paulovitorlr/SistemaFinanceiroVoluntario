namespace CaixaFestejos.Models
{
    public class ItemPedido
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }
        public decimal Custo { get; set; }
        public int Quantidade { get; set; }

        public decimal Subtotal => Preco * Quantidade;
    }
}
