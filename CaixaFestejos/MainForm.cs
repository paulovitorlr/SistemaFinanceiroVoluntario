using CaixaFestejos.Data;
using CaixaFestejos.Models;
using CaixaFestejos.Repositories;
using CaixaFestejos.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CaixaFestejos
{
    public class MainForm : Form
    {
        private static readonly CultureInfo PtBr = new("pt-BR");

        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;
        private readonly IRelatorioService _relatorioService;
        private readonly IExportacaoService _exportacaoService;
        private List<Produto> _produtos = new();
        private readonly List<ItemPedido> _pedidoAtual = new();

        // ---- Controles: aba Vender ----
        private FlowLayoutPanel _painelProdutos = null!;
        private DataGridView _gridPedido = null!;
        private Label _lblTotalPedido = null!;
        private NumericUpDown _numRecebido = null!;
        private Label _lblTroco = null!;
        private Button _btnFinalizar = null!;

        // ---- Controles: aba Produtos ----
        private TextBox _txtNome = null!;
        private NumericUpDown _numPreco = null!;
        private NumericUpDown _numCusto = null!;
        private DataGridView _gridProdutos = null!;

        // ---- Controles: aba Fechamento ----
        private Label _lblTotalVendido = null!;
        private Label _lblItensVendidos = null!;
        private Label _lblRecebido = null!;
        private Label _lblTrocoTotal = null!;
        private Label _lblLucro = null!;
        private Label _lblMaisVendido = null!;
        private DataGridView _gridRelatorio = null!;

        public MainForm()
        {
            var database = new Database();

            _produtoService = new ProdutoService(
                new ProdutoRepository(database));

            _vendaService = new VendaService(
                new VendaRepository(database));

            _relatorioService = new RelatorioService(
                new RelatorioRepository(database));

            _exportacaoService = new ExportacaoService(
                new ExportacaoRepository(database));

            Text = "Caixa dos Festejos";
            Width = 960;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CriarAbaVender());
            tabs.TabPages.Add(CriarAbaProdutos());
            tabs.TabPages.Add(CriarAbaFechamento());
            tabs.SelectedIndexChanged += (s, e) =>
            {
                if (tabs.SelectedIndex == 2)
                    AtualizarFechamento();
            };

            Controls.Add(tabs);

            CarregarProdutos();
        }

        private static string Fmt(decimal valor) => valor.ToString("C2", PtBr);

        // =========================================================
        // ABA VENDER
        // =========================================================
        private TabPage CriarAbaVender()
        {
            var page = new TabPage("Vender");
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
            var grpCardapio = new GroupBox { Text = "Cardápio", Dock = DockStyle.Fill };
            _painelProdutos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8)
            };
            grpCardapio.Controls.Add(_painelProdutos);

            // Pedido atual
            var grpPedido = new GroupBox { Text = "Pedido atual", Dock = DockStyle.Fill };
            var painelPedido = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
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
            var colMenos = new DataGridViewButtonColumn { Name = "Menos", HeaderText = "", Text = "-1", UseColumnTextForButtonValue = true, Width = 40 };
            var colMais = new DataGridViewButtonColumn { Name = "Mais", HeaderText = "", Text = "+1", UseColumnTextForButtonValue = true, Width = 40 };
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
                Text = "Total: " + Fmt(0)
            };

            painelPedido.Controls.Add(_gridPedido, 0, 0);
            painelPedido.Controls.Add(_lblTotalPedido, 0, 1);
            grpPedido.Controls.Add(painelPedido);

            // Pagamento
            var grpPagamento = new GroupBox { Text = "Pagamento", Dock = DockStyle.Fill };
            var painelPagamento = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(8) };
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            painelPagamento.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            var lblRecebido = new Label { Text = "Valor recebido (R$):", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 12, 8, 0) };
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
            _numRecebido.ValueChanged += (s, e) => AtualizarTroco();

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
            painelPagamento.Controls.Add(_lblTroco, 2, 0);
            painelPagamento.Controls.Add(_btnFinalizar, 3, 0);
            grpPagamento.Controls.Add(painelPagamento);

            layout.Controls.Add(grpCardapio, 0, 0);
            layout.Controls.Add(grpPedido, 0, 1);
            layout.Controls.Add(grpPagamento, 0, 2);
            page.Controls.Add(layout);
            return page;
        }

        private void CarregarProdutos()
        {
            _produtos = _produtoService.ListarProdutos();
            RenderCardapio();
            RenderGridProdutos();
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
                    Text = $"{produto.Nome}\n{Fmt(produto.Preco)}",
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
                if (produto == null) return;
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
            if (e.RowIndex < 0) return;
            var colName = _gridPedido.Columns[e.ColumnIndex].Name;
            if (colName != "Menos" && colName != "Mais") return;

            var item = _pedidoAtual[e.RowIndex];
            if (colName == "Mais") item.Quantidade++;
            else item.Quantidade--;

            if (item.Quantidade <= 0) _pedidoAtual.RemoveAt(e.RowIndex);
            RenderPedido();
        }

        private void RenderPedido()
        {
            _gridPedido.Rows.Clear();
            foreach (var item in _pedidoAtual)
            {
                _gridPedido.Rows.Add(item.Nome, item.Quantidade, Fmt(item.Subtotal), "-1", "+1");
            }
            _lblTotalPedido.Text = "Total: " + Fmt(TotalPedido());
            AtualizarTroco();
        }

        private decimal TotalPedido() => _pedidoAtual.Sum(i => i.Subtotal);

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

            decimal troco = recebido - total;
            if (troco < 0)
            {
                _lblTroco.ForeColor = Color.FromArgb(178, 58, 52);
                _lblTroco.Text = "Faltam " + Fmt(-troco);
                _btnFinalizar.Enabled = false;
            }
            else
            {
                _lblTroco.ForeColor = Color.FromArgb(76, 122, 61);
                _lblTroco.Text = "Troco: " + Fmt(troco);
                _btnFinalizar.Enabled = true;
            }
        }

        private void BtnFinalizar_Click(object? sender, EventArgs e)
        {
            try
            {
                _vendaService.RegistrarVenda(
                    _pedidoAtual,
                    _numRecebido.Value);

                MessageBox.Show(
                    this,
                    "Venda registrada com sucesso.",
                    "Venda finalizada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _pedidoAtual.Clear();
                _numRecebido.Value = 0;
                RenderPedido();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // =========================================================
        // ABA PRODUTOS
        // =========================================================
        private TabPage CriarAbaProdutos()
        {
            var page = new TabPage("Produtos");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var grpCadastro = new GroupBox { Text = "Cadastrar produto", Dock = DockStyle.Fill, Height = 110 };
            var painelForm = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, Padding = new Padding(8) };
            painelForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            painelForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            painelForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            painelForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            painelForm.Controls.Add(new Label { Text = "Nome", AutoSize = true }, 0, 0);
            painelForm.Controls.Add(new Label { Text = "Preço (R$)", AutoSize = true }, 1, 0);
            painelForm.Controls.Add(new Label { Text = "Custo (R$) - opcional", AutoSize = true }, 2, 0);

            _txtNome = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 8, 8) };
            _numPreco = new NumericUpDown { DecimalPlaces = 2, Maximum = 100000, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 8, 8) };
            _numCusto = new NumericUpDown { DecimalPlaces = 2, Maximum = 100000, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 8, 8) };
            var btnAdicionar = new Button
            {
                Text = "Adicionar produto",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 8),
                BackColor = Color.FromArgb(35, 52, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdicionar.Click += BtnAdicionarProduto_Click;

            painelForm.Controls.Add(_txtNome, 0, 1);
            painelForm.Controls.Add(_numPreco, 1, 1);
            painelForm.Controls.Add(_numCusto, 2, 1);
            painelForm.Controls.Add(btnAdicionar, 3, 1);
            grpCadastro.Controls.Add(painelForm);

            var grpLista = new GroupBox { Text = "Produtos cadastrados", Dock = DockStyle.Fill };
            _gridProdutos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            _gridProdutos.Columns.Add("Nome", "Nome");
            _gridProdutos.Columns.Add("Preco", "Preço");
            _gridProdutos.Columns.Add("Custo", "Custo");
            var colExcluir = new DataGridViewButtonColumn { Name = "Excluir", HeaderText = "", Text = "Excluir", UseColumnTextForButtonValue = true, Width = 80 };
            _gridProdutos.Columns.Add(colExcluir);
            _gridProdutos.CellContentClick += GridProdutos_CellContentClick;
            grpLista.Controls.Add(_gridProdutos);

            layout.Controls.Add(grpCadastro, 0, 0);
            layout.Controls.Add(grpLista, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private void BtnAdicionarProduto_Click(object? sender, EventArgs e)
        {
            string nome = _txtNome.Text.Trim();
            decimal preco = _numPreco.Value;
            decimal custo = _numCusto.Value;

            if (string.IsNullOrWhiteSpace(nome) || preco <= 0)
            {
                MessageBox.Show(this, "Preencha nome e preço corretamente.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _produtoService.AdicionarProduto(new Produto
            {
                Nome = nome,
                Preco = preco,
                Custo = custo
            });
            _txtNome.Text = "";
            _numPreco.Value = 0;
            _numCusto.Value = 0;
            CarregarProdutos();
        }

        private void GridProdutos_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_gridProdutos.Columns[e.ColumnIndex].Name != "Excluir") return;

            var produto = _produtos[e.RowIndex];
            var resultado = MessageBox.Show(this, $"Excluir \"{produto.Nome}\"?", "Confirmar exclusão",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resultado == DialogResult.Yes)
            {
                _produtoService.ExcluirProduto(produto.Id);
                CarregarProdutos();
            }
        }

        private void RenderGridProdutos()
        {
            _gridProdutos.Rows.Clear();
            foreach (var produto in _produtos)
            {
                _gridProdutos.Rows.Add(produto.Nome, Fmt(produto.Preco), Fmt(produto.Custo), "Excluir");
            }
        }

        // =========================================================
        // ABA FECHAMENTO
        // =========================================================
        private TabPage CriarAbaFechamento()
        {
            var page = new TabPage("Fechamento");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var grpMetricas = new GroupBox { Text = "Resumo", Dock = DockStyle.Fill, Height = 140 };
            var metricas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(10) };
            for (int i = 0; i < 3; i++) metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));

            (_lblTotalVendido, var p1) = CriarMetrica("Total vendido");
            (_lblItensVendidos, var p2) = CriarMetrica("Itens vendidos");
            (_lblRecebido, var p3) = CriarMetrica("Dinheiro recebido");
            (_lblTrocoTotal, var p4) = CriarMetrica("Troco devolvido");
            (_lblLucro, var p5) = CriarMetrica("Lucro estimado");
            (_lblMaisVendido, var p6) = CriarMetrica("Mais vendido");

            metricas.Controls.Add(p1, 0, 0);
            metricas.Controls.Add(p2, 1, 0);
            metricas.Controls.Add(p3, 2, 0);
            metricas.Controls.Add(p4, 0, 1);
            metricas.Controls.Add(p5, 1, 1);
            metricas.Controls.Add(p6, 2, 1);
            grpMetricas.Controls.Add(metricas);

            var grpTabela = new GroupBox { Text = "Vendas por produto", Dock = DockStyle.Fill };
            _gridRelatorio = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridRelatorio.Columns.Add("Nome", "Produto");
            _gridRelatorio.Columns.Add("Qtd", "Quantidade");
            _gridRelatorio.Columns.Add("Total", "Total");
            grpTabela.Controls.Add(_gridRelatorio);

            var painelBotoes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            var btnAtualizar = new Button { Text = "Atualizar", Width = 110, Height = 34 };
            var btnExportar = new Button { Text = "Exportar CSV", Width = 130, Height = 34 };
            var btnZerar = new Button { Text = "Zerar vendas", Width = 130, Height = 34, ForeColor = Color.FromArgb(178, 58, 52) };
            btnAtualizar.Click += (s, e) => AtualizarFechamento();
            btnExportar.Click += BtnExportarCsv_Click;
            btnZerar.Click += BtnZerarVendas_Click;
            painelBotoes.Controls.Add(btnAtualizar);
            painelBotoes.Controls.Add(btnExportar);
            painelBotoes.Controls.Add(btnZerar);

            layout.Controls.Add(grpMetricas, 0, 0);
            layout.Controls.Add(grpTabela, 0, 1);
            layout.Controls.Add(painelBotoes, 0, 2);
            page.Controls.Add(layout);
            return page;
        }

        private (Label valor, Panel painel) CriarMetrica(string titulo)
        {
            var painel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(255, 253, 247), Margin = new Padding(4) };
            var lblTitulo = new Label { Text = titulo, Dock = DockStyle.Top, Height = 20, ForeColor = Color.Gray, Padding = new Padding(8, 6, 0, 0) };
            var lblValor = new Label { Text = "—", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13, FontStyle.Bold), Padding = new Padding(8, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft };
            painel.Controls.Add(lblTitulo);
            painel.Controls.Add(lblValor);
            return (lblValor, painel);
        }

        private void AtualizarFechamento()
        {
            var resumo = _relatorioService.ObterResumo();
            _lblTotalVendido.Text = Fmt(resumo.TotalVendido);
            _lblItensVendidos.Text = resumo.ItensVendidos.ToString();
            _lblRecebido.Text = Fmt(resumo.TotalRecebido);
            _lblTrocoTotal.Text = Fmt(resumo.TotalTroco);
            _lblLucro.Text = Fmt(resumo.Lucro);
            _lblMaisVendido.Text = resumo.MaisVendido;

            _gridRelatorio.Rows.Clear();
            foreach (var item in _relatorioService.ObterVendasPorProduto())
            {
                _gridRelatorio.Rows.Add(item.Nome, item.Quantidade, Fmt(item.Total));
            }
        }

        private void BtnExportarCsv_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Arquivo CSV (*.csv)|*.csv",
                FileName = $"vendas-festejos-{DateTime.Now:yyyy-MM-dd}.csv"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _exportacaoService.ExportarCsv(dialog.FileName);
                MessageBox.Show(this, "Relatório exportado com sucesso.", "Exportado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnZerarVendas_Click(object? sender, EventArgs e)
        {
            var resultado = MessageBox.Show(this,
                "Isso vai apagar todas as vendas registradas (os produtos continuam cadastrados). Confirma?",
                "Zerar vendas", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (resultado == DialogResult.Yes)
            {
                _vendaService.ZerarVendas();
                AtualizarFechamento();
                MessageBox.Show(this, "Vendas zeradas. Pronto para o próximo festejo.", "Concluído",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
