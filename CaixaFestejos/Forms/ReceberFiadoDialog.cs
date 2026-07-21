using CaixaFestejos.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CaixaFestejos.Forms
{
    /// <summary>
    /// Diálogo simples para confirmar o valor recebido e a forma de pagamento
    /// no momento de quitar um fiado. Não acessa Service/Repository — só coleta
    /// a entrada do usuário, quem valida e efetiva é o FiadoService.
    /// </summary>
    public class ReceberFiadoDialog : Form
    {
        private readonly NumericUpDown _numValor;
        private readonly ComboBox _cmbFormaPagamento;

        public decimal ValorRecebido => _numValor.Value;
        public FormaPagamento FormaPagamento => (FormaPagamento)_cmbFormaPagamento.SelectedItem!;

        public ReceberFiadoDialog(string cliente, decimal valorDevido)
        {
            Text = "Receber pagamento";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 340;
            Height = 220;
            Font = new Font("Segoe UI", 9.5f);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };

            var lblCliente = new Label
            {
                Text = $"Cliente: {cliente}\nValor devido: {valorDevido:C2}",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };

            var lblValor = new Label { Text = "Valor recebido (R$):", AutoSize = true };
            _numValor = new NumericUpDown
            {
                DecimalPlaces = 2,
                Maximum = 1000000,
                Minimum = 0,
                Width = 150,
                Value = valorDevido,
                Margin = new Padding(0, 4, 0, 12)
            };

            var lblForma = new Label { Text = "Forma de pagamento:", AutoSize = true };
            _cmbFormaPagamento = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                Margin = new Padding(0, 4, 0, 16)
            };
            _cmbFormaPagamento.Items.Add(FormaPagamento.Especie);
            _cmbFormaPagamento.Items.Add(FormaPagamento.Pix);
            _cmbFormaPagamento.SelectedIndex = 0;

            var painelBotoes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            var btnConfirmar = new Button
            {
                Text = "Confirmar",
                Width = 100,
                Height = 32,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(35, 52, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnCancelar = new Button { Text = "Cancelar", Width = 90, Height = 32, DialogResult = DialogResult.Cancel };

            painelBotoes.Controls.Add(btnConfirmar);
            painelBotoes.Controls.Add(btnCancelar);

            layout.Controls.Add(lblCliente);
            layout.Controls.Add(lblValor);
            layout.Controls.Add(_numValor);
            layout.Controls.Add(lblForma);
            layout.Controls.Add(_cmbFormaPagamento);
            layout.Controls.Add(painelBotoes);

            Controls.Add(layout);
            AcceptButton = btnConfirmar;
            CancelButton = btnCancelar;
        }
    }
}
