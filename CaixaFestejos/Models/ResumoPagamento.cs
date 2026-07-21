namespace CaixaFestejos.Models;

public class ResumoPagamento
{
    public decimal TotalGeral { get; set; }

    public decimal TotalEspecie { get; set; }

    public decimal TotalPix { get; set; }

    /// <summary>Soma dos fiados ainda em aberto (não pagos). Não faz parte do TotalGeral,
    /// pois fiado é dívida pendente, não dinheiro recebido no caixa.</summary>
    public decimal TotalFiado { get; set; }

}