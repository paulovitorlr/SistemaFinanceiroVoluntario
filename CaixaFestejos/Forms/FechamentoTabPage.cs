using CaixaFestejos.Models;
using CaixaFestejos.Services;
using CaixaFestejos.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CaixaFestejos.Forms
{
    /// <summary>
    /// Aba "Fechamento": resumo do caixa, vendas por produto, exportação e zerar vendas.
    /// </summary>
    public class FechamentoTabPage : TabPage
    {
        private readonly IRelatorioService _relatorioService;
        private readonly IVendaService _vendaService;
        private readonly IExportacaoService _exportacaoService;
        private readonly IProdutoService _produtoService;

        private Label _lblTotalVendido = null!;
        private Label _lblItensVendidos = null!;
        private Label _lblRecebido = null!;
        private Label _lblTrocoTotal = null!;
        private Label _lblLucro = null!;
        private Label _lblMaisVendido = null!;
        private DataGridView _gridRelatorio = null!;
        private DataGridView _gridVendas = null!;

        public FechamentoTabPage(
            IRelatorioService relatorioService,
            IVendaService vendaService,
            IExportacaoService exportacaoService,
            IProdutoService produtoService)
            : base("Fechamento")
        {
            _relatorioService = relatorioService;
            _vendaService = vendaService;
            _exportacaoService = exportacaoService;
            _produtoService = produtoService;

            ConstruirLayout();
        }

        private void ConstruirLayout()
        {
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

            var grpVendas = new GroupBox { Text = "Vendas registradas (corrigir venda já finalizada)", Dock = DockStyle.Fill };
            _gridVendas = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridVendas.Columns.Add("Id", "#");
            _gridVendas.Columns.Add("Data", "Data/Hora");
            _gridVendas.Columns.Add("Total", "Total");
            _gridVendas.Columns.Add("FormaPagamento", "Pagamento");
            _gridVendas.Columns["Id"]!.ReadOnly = true;
            _gridVendas.Columns["Data"]!.ReadOnly = true;
            _gridVendas.Columns["Total"]!.ReadOnly = true;
            _gridVendas.Columns["FormaPagamento"]!.ReadOnly = true;

            var colEditarVenda = new DataGridViewButtonColumn
            {
                Name = "Editar",
                HeaderText = "",
                Text = "Editar itens",
                UseColumnTextForButtonValue = true,
                Width = 90
            };
            _gridVendas.Columns.Add(colEditarVenda);
            _gridVendas.CellContentClick += GridVendas_CellContentClick;
            grpVendas.Controls.Add(_gridVendas);

            var painelTabelas = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            painelTabelas.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            painelTabelas.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            painelTabelas.Controls.Add(grpTabela, 0, 0);
            painelTabelas.Controls.Add(grpVendas, 0, 1);

            var painelBotoes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            var btnAtualizar = new Button { Text = "Atualizar", Width = 110, Height = 34 };
            var btnExportar = new Button { Text = "Exportar CSV", Width = 130, Height = 34 };
            var btnZerar = new Button { Text = "Zerar vendas", Width = 130, Height = 34, ForeColor = Color.FromArgb(178, 58, 52) };
            btnAtualizar.Click += (s, e) => Atualizar();
            btnExportar.Click += BtnExportarCsv_Click;
            btnZerar.Click += BtnZerarVendas_Click;
            painelBotoes.Controls.Add(btnAtualizar);
            painelBotoes.Controls.Add(btnExportar);
            painelBotoes.Controls.Add(btnZerar);

            layout.Controls.Add(grpMetricas, 0, 0);
            layout.Controls.Add(painelTabelas, 0, 1);
            layout.Controls.Add(painelBotoes, 0, 2);
            Controls.Add(layout);
        }

        private void GridVendas_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (_gridVendas.Columns[e.ColumnIndex].Name != "Editar")
                return;

            var vendaId = (int)_gridVendas.Rows[e.RowIndex].Cells["Id"].Value!;
            var venda = _vendaService.ObterVenda(vendaId);

            if (venda == null)
            {
                MessageBox.Show(FindForm(), "Venda não encontrada (pode já ter sido excluída ou zerada).",
                    "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Atualizar();
                return;
            }

            using var dialog = new EditarVendaDialog(_vendaService, _produtoService, new PedidoService(), venda);

            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                Atualizar();
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

        /// <summary>
        /// Recarrega o resumo e a tabela de vendas por produto.
        /// Deve ser chamado pelo MainForm sempre que esta aba for selecionada.
        /// </summary>
        public void Atualizar()
        {
            var resumo = _relatorioService.ObterResumo();
            _lblTotalVendido.Text = Formatador.Moeda(resumo.TotalVendido);
            _lblItensVendidos.Text = resumo.ItensVendidos.ToString();
            _lblRecebido.Text = Formatador.Moeda(resumo.TotalRecebido);
            _lblTrocoTotal.Text = Formatador.Moeda(resumo.TotalTroco);
            _lblLucro.Text = Formatador.Moeda(resumo.Lucro);
            _lblMaisVendido.Text = resumo.MaisVendido;

            _gridRelatorio.Rows.Clear();
            foreach (var item in _relatorioService.ObterVendasPorProduto())
            {
                _gridRelatorio.Rows.Add(item.Nome, item.Quantidade, Formatador.Moeda(item.Total));
            }

            _gridVendas.Rows.Clear();
            foreach (var venda in _vendaService.ListarVendas())
            {
                _gridVendas.Rows.Add(
                    venda.Id,
                    venda.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    Formatador.Moeda(venda.Total),
                    venda.FormaPagamento.ToString(),
                    "Editar itens");
            }
        }

        private void BtnExportarCsv_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Planilha do Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true,
                FileName = $"vendas-festejos-{DateTime.Now:yyyy-MM-dd}.xlsx"
            };

            if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _exportacaoService.ExportarCsv(dialog.FileName);

                MessageBox.Show(
                    FindForm(),
                    "Relatório exportado com sucesso.",
                    "Exportado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void BtnZerarVendas_Click(object? sender, EventArgs e)
        {
            var resultado = MessageBox.Show(FindForm(),
                "Isso vai apagar todas as vendas registradas (os produtos continuam cadastrados). Confirma?",
                "Zerar vendas", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (resultado == DialogResult.Yes)
            {
                _vendaService.ZerarVendas();
                Atualizar();
                MessageBox.Show(FindForm(), "Vendas zeradas. Pronto para o próximo festejo.", "Concluído",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}