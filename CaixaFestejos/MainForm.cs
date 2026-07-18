using CaixaFestejos.Data;
using CaixaFestejos.Forms;
using CaixaFestejos.Repositories;
using CaixaFestejos.Services;
using System.Drawing;
using System.Windows.Forms;

namespace CaixaFestejos
{
    /// <summary>
    /// Motor da aplicação: monta a infraestrutura (banco, repositórios, serviços),
    /// instancia as abas (cada uma responsável pela própria UI/lógica) e conecta
    /// os eventos entre elas. Não contém lógica de negócio nem de UI específica de aba.
    /// </summary>
    public class MainForm : Form
    {
        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;
        private readonly IRelatorioService _relatorioService;
        private readonly IExportacaoService _exportacaoService;

        private VenderTabPage _abaVender = null!;
        private ProdutosTabPage _abaProdutos = null!;
        private FechamentoTabPage _abaFechamento = null!;

        public MainForm()
        {
            var database = new Database();

            var initializer = new DatabaseInitializer(database);
            initializer.Inicializar();

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

            Controls.Add(CriarTabs());
        }

        private TabControl CriarTabs()
        {
            _abaVender = new VenderTabPage(_produtoService, _vendaService);
            _abaProdutos = new ProdutosTabPage(_produtoService);
            _abaFechamento = new FechamentoTabPage(_relatorioService, _vendaService, _exportacaoService);

            // Quando o catálogo muda na aba Produtos, o cardápio da aba Vender precisa refletir isso.
            _abaProdutos.ProdutosAlterados += (s, e) => _abaVender.AtualizarCardapio();

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(_abaVender);
            tabs.TabPages.Add(_abaProdutos);
            tabs.TabPages.Add(_abaFechamento);

            // Ao entrar na aba Fechamento, o resumo é recalculado.
            tabs.SelectedIndexChanged += (s, e) =>
            {
                if (tabs.SelectedTab == _abaFechamento)
                    _abaFechamento.Atualizar();
            };

            return tabs;
        }
    }
}