using CaixaFestejos.Models;
using CaixaFestejos.Services;
using CaixaFestejos.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CaixaFestejos.Forms
{
    /// <summary>
    /// Diálogo modal para corrigir os itens de uma venda já finalizada (registrada no banco).
    /// Reaproveita o mesmo IPedidoService usado na aba "Vender" para montar o pedido em memória;
    /// a diferença é que aqui ele começa pré-carregado com os itens da venda e, ao salvar, a
    /// alteração é persistida via IVendaService.EditarVenda em vez de RegistrarVenda.
    /// Não altera o cadastro de produtos, e não muda o valor recebido nem a forma de pagamento
    /// originais da venda — só corrige quais produtos/quantidades entraram nela.
    /// </summary>
    public class EditarVendaDialog : Form
    {
        private readonly IVendaService _vendaService;
        private readonly IProdutoService _produtoService;
        private readonly IPedidoService _pedidoService;
        private readonly int _vendaId;

        private FlowLayoutPanel _painelProdutos = null!;
        private DataGridView _gridPedido = null!;
        private Label _lblTotal = null!;

        public EditarVendaDialog(
            IVendaService vendaService,
            IProdutoService produtoService,
            IPedidoService pedidoService,
            Venda venda)
        {
            _vendaService = vendaService;
            _produtoService = produtoService;
            _pedidoService = pedidoService;
            _vendaId = venda.Id;

            Text = $"Editar venda #{venda.Id} — {venda.DataHora:dd/MM/yyyy HH:mm}";
            Width = 720;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;

            // Carrega os itens já salvos dessa venda no serviço de pedido (em memória).
            _pedidoService.CarregarItens(venda.Itens.Select(i => new ItemPedido
            {
                ProdutoId = i.ProdutoId,
                Nome = i.Nome,
                Preco = i.Preco,
                Custo = i.Custo,
                Quantidade = i.Quantidade
            }));

            ConstruirLayout();
            RenderCardapio();
            RenderPedido();
        }

        private void ConstruirLayout()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var grpCardapio = new GroupBox { Text = "Adicionar produto", Dock = DockStyle.Fill };
            _painelProdutos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            grpCardapio.Controls.Add(_painelProdutos);

            var grpPedido = new GroupBox { Text = "Itens da venda", Dock = DockStyle.Fill };

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
            _gridPedido.Columns["Produto"]!.ReadOnly = true;
            _gridPedido.Columns["Qtd"]!.ReadOnly = true;
            _gridPedido.Columns["Subtotal"]!.ReadOnly = true;

            var colEditar = new DataGridViewButtonColumn
            {
                Name = "Editar",
                HeaderText = "",
                Text = "Editar qtd.",
                UseColumnTextForButtonValue = true,
                Width = 90
            };

            var colRemover = new DataGridViewButtonColumn
            {
                Name = "Remover",
                HeaderText = "",
                Text = "Remover",
                UseColumnTextForButtonValue = true,
                Width = 80
            };

            _gridPedido.Columns.Add(colEditar);
            _gridPedido.Columns.Add(colRemover);
            _gridPedido.CellContentClick += GridPedido_CellContentClick;

            grpPedido.Controls.Add(_gridPedido);

            _lblTotal = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Text = "Total: " + Formatador.Moeda(0)
            };

            var painelBotoes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            var btnSalvar = new Button
            {
                Text = "Salvar alterações",
                Width = 150,
                Height = 34,
                BackColor = Color.FromArgb(76, 122, 61),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.Click += BtnSalvar_Click;

            var btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 34 };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            painelBotoes.Controls.Add(btnSalvar);
            painelBotoes.Controls.Add(btnCancelar);

            layout.Controls.Add(grpCardapio, 0, 0);
            layout.Controls.Add(grpPedido, 0, 1);
            layout.Controls.Add(_lblTotal, 0, 2);
            layout.Controls.Add(painelBotoes, 0, 3);

            Controls.Add(layout);
        }

        private void RenderCardapio()
        {
            _painelProdutos.Controls.Clear();

            foreach (var produto in _produtoService.ListarProdutos())
            {
                var btn = new Button
                {
                    Text = $"{produto.Nome}\n{Formatador.Moeda(produto.Preco)}",
                    Width = 140,
                    Height = 55,
                    Margin = new Padding(5),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(255, 253, 247)
                };

                btn.Click += (s, e) =>
                {
                    _pedidoService.AdicionarItem(produto);
                    RenderPedido();
                };

                _painelProdutos.Controls.Add(btn);
            }
        }

        private void GridPedido_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var colName = _gridPedido.Columns[e.ColumnIndex].Name;

            if (colName != "Editar" && colName != "Remover")
                return;

            var item = _pedidoService.Itens[e.RowIndex];

            try
            {
                if (colName == "Editar")
                {
                    var entrada = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Nova quantidade para \"{item.Nome}\":",
                        "Editar quantidade",
                        item.Quantidade.ToString());

                    if (string.IsNullOrWhiteSpace(entrada))
                        return;

                    if (!int.TryParse(entrada, out var novaQuantidade))
                    {
                        MessageBox.Show(this, "Informe um número inteiro válido.", "Atenção",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _pedidoService.AlterarQuantidadeItem(item.ProdutoId, novaQuantidade);
                }
                else
                {
                    var confirmar = MessageBox.Show(this, $"Remover \"{item.Nome}\" desta venda?",
                        "Remover item", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirmar == DialogResult.Yes)
                        _pedidoService.RemoverItemPedido(item.ProdutoId);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RenderPedido();
        }

        private void RenderPedido()
        {
            _gridPedido.Rows.Clear();

            foreach (var item in _pedidoService.Itens)
            {
                _gridPedido.Rows.Add(
                    item.Nome,
                    item.Quantidade,
                    Formatador.Moeda(item.Subtotal),
                    "Editar qtd.",
                    "Remover");
            }

            _lblTotal.Text = "Total: " + Formatador.Moeda(_pedidoService.RecalcularPedido());
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            try
            {
                _vendaService.EditarVenda(_vendaId, _pedidoService.Itens.ToList());

                MessageBox.Show(this, "Venda atualizada com sucesso.", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
