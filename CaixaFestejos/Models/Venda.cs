namespace CaixaFestejos.Models;
public class Venda
{
    public int Id { get; set; }

    public DateTime DataHora { get; set; }

    public decimal Total { get; set; }

    public decimal CustoTotal { get; set; }

    public decimal Recebido { get; set; }

    public decimal Troco { get; set; }
    public FormaPagamento FormaPagamento { get; set; }

    public List<ItemVenda> Itens { get; set; } = new();
}