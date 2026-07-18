using CaixaFestejos.Models;
using CaixaFestejos.Services;
using CaixaFestejos.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CaixaFestejos.Forms
{
    /// <summary>
    /// Aba "Produtos": cadastro e listagem/exclusão de produtos.
    /// Dispara <see cref="ProdutosAlterados"/> sempre que o catálogo muda,
    /// para que outras abas (ex.: Vender) possam se atualizar.
    /// </summary>
    public class ProdutosTabPage : TabPage
    {
        private readonly IProdutoService _produtoService;
        private List<Produto> _produtos = new();

        private TextBox _txtNome = null!;
        private NumericUpDown _numPreco = null!;
        private NumericUpDown _numCusto = null!;
        private DataGridView _gridProdutos = null!;

        /// <summary>Disparado após qualquer inclusão ou exclusão de produto.</summary>
        public event EventHandler? ProdutosAlterados;

        public ProdutosTabPage(IProdutoService produtoService)
            : base("Produtos")
        {
            _produtoService = produtoService;

            ConstruirLayout();
            CarregarProdutos();
        }

        private void ConstruirLayout()
        {
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
            Controls.Add(layout);
        }

        private void CarregarProdutos()
        {
            _produtos = _produtoService.ListarProdutos();
            RenderGridProdutos();
        }

        private void BtnAdicionarProduto_Click(object? sender, EventArgs e)
        {
            string nome = _txtNome.Text.Trim();
            decimal preco = _numPreco.Value;
            decimal custo = _numCusto.Value;

            if (string.IsNullOrWhiteSpace(nome) || preco <= 0)
            {
                MessageBox.Show(FindForm(), "Preencha nome e preço corretamente.", "Atenção",
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
            ProdutosAlterados?.Invoke(this, EventArgs.Empty);
        }

        private void GridProdutos_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_gridProdutos.Columns[e.ColumnIndex].Name != "Excluir") return;

            var produto = _produtos[e.RowIndex];
            var resultado = MessageBox.Show(FindForm(), $"Excluir \"{produto.Nome}\"?", "Confirmar exclusão",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resultado == DialogResult.Yes)
            {
                _produtoService.ExcluirProduto(produto.Id);
                CarregarProdutos();
                ProdutosAlterados?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RenderGridProdutos()
        {
            _gridProdutos.Rows.Clear();
            foreach (var produto in _produtos)
            {
                _gridProdutos.Rows.Add(produto.Nome, Formatador.Moeda(produto.Preco), Formatador.Moeda(produto.Custo), "Excluir");
            }
        }
    }
}