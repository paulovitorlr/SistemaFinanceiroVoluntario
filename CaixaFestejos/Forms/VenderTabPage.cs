using CaixaFestejos.Models;
using CaixaFestejos.Services;
using CaixaFestejos.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CaixaFestejos.Forms
{
    /// <summary>
    /// Aba "Vender": cardápio, pedido atual e pagamento.
    /// Toda a lógica de montagem de pedido e finalização de venda vive aqui.
    /// </summary>
    public class VenderTabPage : TabPage
    {
        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;

        private List<Produto> _produtos = new();
        private readonly List<ItemPedido> _pedidoAtual = new();

        private FlowLayoutPanel _painelProdutos = null!;
        private DataGridView _gridPedido = null!;
        private Label _lblTotalPedido = null!;
        private NumericUpDown _numRecebido = null!;
        private ComboBox _cmbFormaPagamento = null!;
        private Label _lblTroco = null!;
        private Button _btnFinalizar = null!;
        private string? _clienteFiado;
        private readonly decimal[] _cedulasRapidas =
        {
            2,
            5,
            10,
            20
        };

        public VenderTabPage(IProdutoService produtoService, IVendaService vendaService)
            : base("Vender")
        {
            _produtoService = produtoService;
            _vendaService = vendaService;

            ConstruirLayout();
            AtualizarCardapio();
        }

        /// <summary>
        /// Recarrega a lista de produtos a partir do serviço e redesenha o cardápio.
        /// Deve ser chamado pelo MainForm sempre que a aba Produtos alterar o catálogo.
        /// </summary>
        public void AtualizarCardapio()
        {
            _produtos = _produtoService.ListarProdutos();
            RenderCardapio();
        }

        private Button CriarBotaoCedula(decimal valor)
        {
            var btn = new Button
            {
                Text = Formatador.Moeda(valor),
                Width = 70,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3)
            };

            btn.Click += (s, e) =>
            {
                _numRecebido.Value = valor;
            };

            return btn;
        }

        private void ConstruirLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            // Cardápio
            var grpCardapio = new GroupBox
            {
                Text = "Cardápio",
                Dock = DockStyle.Fill
            };

            _painelProdutos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8)
            };

            grpCardapio.Controls.Add(_painelProdutos);

            // Pedido
            var grpPedido = new GroupBox
            {
                Text = "Pedido atual",
                Dock = DockStyle.Fill
            };

            var painelPedido = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };

            painelPedido.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
            painelPedido.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            _gridPedido = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _gridPedido.Columns.Add("Produto", "Produto");
            _gridPedido.Columns.Add("Qtd", "Quantidade");
            _gridPedido.Columns.Add("Subtotal", "Subtotal");

            var colMenos = new DataGridViewButtonColumn
            {
                Name = "Menos",
                HeaderText = "",
                Text = "-1",
                UseColumnTextForButtonValue = true,
                Width = 40
            };

            var colMais = new DataGridViewButtonColumn
            {
                Name = "Mais",
                HeaderText = "",
                Text = "+1",
                UseColumnTextForButtonValue = true,
                Width = 40
            };

            _gridPedido.Columns.Add(colMenos);
            _gridPedido.Columns.Add(colMais);

            _gridPedido.Columns["Produto"]!.ReadOnly = true;
            _gridPedido.Columns["Qtd"]!.ReadOnly = true;
            _gridPedido.Columns["Subtotal"]!.ReadOnly = true;

            _gridPedido.CellContentClick += GridPedido_CellContentClick;

            _lblTotalPedido = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Text = "Total: " + Formatador.Moeda(0)
            };

            painelPedido.Controls.Add(_gridPedido, 0, 0);
            painelPedido.Controls.Add(_lblTotalPedido, 0, 1);

            grpPedido.Controls.Add(painelPedido);

            // Pagamento
            var grpPagamento = new GroupBox
            {
                Text = "Pagamento",
                Dock = DockStyle.Fill
            };

            var painelPagamento = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                Padding = new Padding(8)
            };
            painelPagamento.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            painelPagamento.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            var lblRecebido = new Label
            {
                Text = "Valor recebido (R$):",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(0, 12, 8, 0)
            };

            _numRecebido = new NumericUpDown
            {
                DecimalPlaces = 2,
                Maximum = 100000,
                Minimum = 0,
                Increment = 1,
                Width = 110,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0)
            };
            var painelCaixaRapido = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                WrapContents = false
            };

            foreach (var cedula in _cedulasRapidas)
            {
                painelCaixaRapido.Controls.Add(
                    CriarBotaoCedula(cedula)
                );
            }

            _numRecebido.ValueChanged += (s, e) => AtualizarTroco();

            _cmbFormaPagamento = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Anchor = AnchorStyles.Left
            };

            _cmbFormaPagamento.Items.Add(FormaPagamento.Especie);
            _cmbFormaPagamento.Items.Add(FormaPagamento.Pix);
            _cmbFormaPagamento.Items.Add(FormaPagamento.Fiado);
            _cmbFormaPagamento.SelectedIndex = 0;

            _cmbFormaPagamento.SelectedIndexChanged += (s, e) =>
            {
                AtualizarTroco();
            };

            _lblTroco = new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "",
                Margin = new Padding(20, 12, 0, 0)
            };

            _btnFinalizar = new Button
            {
                Text = "Finalizar venda",
                Dock = DockStyle.Fill,
                Height = 40,
                Enabled = false,
                BackColor = Color.FromArgb(35, 52, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _btnFinalizar.Click += BtnFinalizar_Click;

            painelPagamento.Controls.Add(lblRecebido, 0, 0);
            painelPagamento.Controls.Add(_numRecebido, 1, 0);
            painelPagamento.Controls.Add(painelCaixaRapido, 0, 1);
            painelPagamento.SetColumnSpan(painelCaixaRapido, 4); // ocupa da coluna 0 até a 3
            painelPagamento.Controls.Add(_cmbFormaPagamento, 2, 0);
            painelPagamento.Controls.Add(_lblTroco, 3, 0);
            painelPagamento.Controls.Add(_btnFinalizar, 4, 0);

            grpPagamento.Controls.Add(painelPagamento);

            layout.Controls.Add(grpCardapio, 0, 0);
            layout.Controls.Add(grpPedido, 0, 1);
            layout.Controls.Add(grpPagamento, 0, 2);

            Controls.Add(layout);

        }
        private void RenderCardapio()
        {
            _painelProdutos.Controls.Clear();

            if (_produtos.Count == 0)
            {
                _painelProdutos.Controls.Add(new Label
                {
                    Text = "Cadastre produtos na aba \"Produtos\" para começar a vender.",
                    AutoSize = true,
                    ForeColor = Color.Gray
                });

                return;
            }

            foreach (var produto in _produtos)
            {
                var btn = new Button
                {
                    Text = $"{produto.Nome}\n{Formatador.Moeda(produto.Preco)}",
                    Width = 150,
                    Height = 60,
                    Margin = new Padding(5),
                    Tag = produto.Id,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(255, 253, 247)
                };

                btn.Click += (s, e) => AdicionarAoPedido(produto.Id);

                _painelProdutos.Controls.Add(btn);
            }
        }

        private void AdicionarAoPedido(int produtoId)
        {
            var item = _pedidoAtual.FirstOrDefault(i => i.ProdutoId == produtoId);

            if (item != null)
            {
                item.Quantidade++;
            }
            else
            {
                var produto = _produtos.FirstOrDefault(p => p.Id == produtoId);

                if (produto == null)
                    return;

                _pedidoAtual.Add(new ItemPedido
                {
                    ProdutoId = produto.Id,
                    Nome = produto.Nome,
                    Preco = produto.Preco,
                    Custo = produto.Custo,
                    Quantidade = 1
                });
            }

            RenderPedido();
        }

        private void GridPedido_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var colName = _gridPedido.Columns[e.ColumnIndex].Name;

            if (colName != "Menos" && colName != "Mais")
                return;

            var item = _pedidoAtual[e.RowIndex];

            if (colName == "Mais")
                item.Quantidade++;
            else
                item.Quantidade--;

            if (item.Quantidade <= 0)
                _pedidoAtual.RemoveAt(e.RowIndex);

            RenderPedido();
        }

        private void RenderPedido()
        {
            _gridPedido.Rows.Clear();

            foreach (var item in _pedidoAtual)
            {
                _gridPedido.Rows.Add(
                    item.Nome,
                    item.Quantidade,
                    Formatador.Moeda(item.Subtotal),
                    "-1",
                    "+1");
            }

            _lblTotalPedido.Text = "Total: " + Formatador.Moeda(TotalPedido());

            AtualizarTroco();
        }

        private decimal TotalPedido()
        {
            return _pedidoAtual.Sum(i => i.Subtotal);
        }

        private void AtualizarTroco()
        {
            decimal total = TotalPedido();
            decimal recebido = _numRecebido.Value;

            if (_pedidoAtual.Count == 0)
            {
                _lblTroco.Text = "";
                _btnFinalizar.Enabled = false;
                return;
            }

            var formaPagamento = (FormaPagamento)_cmbFormaPagamento.SelectedItem!;

            // Fiado não exige recebimento no momento da venda
            if (formaPagamento == FormaPagamento.Fiado)
            {
                _lblTroco.ForeColor = Color.FromArgb(76, 122, 61);
                _lblTroco.Text = "Pagamento pendente";
                _btnFinalizar.Enabled = true;
                return;
            }

            decimal troco = recebido - total;

            if (troco < 0)
            {
                _lblTroco.ForeColor = Color.FromArgb(178, 58, 52);
                _lblTroco.Text = "Faltam " + Formatador.Moeda(-troco);
                _btnFinalizar.Enabled = false;
            }
            else
            {
                _lblTroco.ForeColor = Color.FromArgb(76, 122, 61);
                _lblTroco.Text = "Troco: " + Formatador.Moeda(troco);
                _btnFinalizar.Enabled = true;
            }
        }

        private void BtnFinalizar_Click(object? sender, EventArgs e)
        {
            try
            {
                var formaPagamento = (FormaPagamento)_cmbFormaPagamento.SelectedItem!;

                _clienteFiado = null;

                if (formaPagamento == FormaPagamento.Fiado)
                {
                    _clienteFiado = Microsoft.VisualBasic.Interaction.InputBox(
                        "Nome do cliente:",
                        "Venda fiada",
                        "");

                    if (string.IsNullOrWhiteSpace(_clienteFiado))
                    {
                        MessageBox.Show(
                            FindForm(),
                            "Informe o nome do cliente para registrar o fiado.",
                            "Atenção",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }
                }

                _vendaService.RegistrarVenda(
                    _pedidoAtual,
                    _numRecebido.Value,
                    formaPagamento,
                    _clienteFiado);

                MessageBox.Show(
                    FindForm(),
                    "Venda registrada com sucesso.",
                    "Venda finalizada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _pedidoAtual.Clear();

                _numRecebido.Value = 0;
                _cmbFormaPagamento.SelectedIndex = 0;

                _clienteFiado = null;

                RenderPedido();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    FindForm(),
                    ex.Message,
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}