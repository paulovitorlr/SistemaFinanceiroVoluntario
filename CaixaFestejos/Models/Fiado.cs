namespace CaixaFestejos.Models;

public class Fiado
{
    public int Id { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public decimal ValorTotal { get; set; }

    public bool Pago { get; set; }

    public DateTime? DataPagamento { get; set; }

    /// <summary>Forma de pagamento usada na quitação (nula enquanto o fiado está em aberto).</summary>
    public FormaPagamento? FormaPagamentoRecebimento { get; set; }

    public List<ItemFiado> Itens { get; set; } = new();
}