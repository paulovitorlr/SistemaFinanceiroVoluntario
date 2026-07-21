namespace CaixaFestejos.Models;

public class ItemFiado
{
    public int ProdutoId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal CustoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;

    public decimal CustoTotal => Quantidade * CustoUnitario;
}