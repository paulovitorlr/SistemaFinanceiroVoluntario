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
    /// Aba "Fiados": lista fiados pendentes e quitados, permite receber pagamento
    /// e excluir um fiado. Não mistura fiado com venda normal — fiado é dívida
    /// pendente, só vira "recebido" quando o Service confirma a quitação.
    /// </summary>
    public class FiadosTabPage : TabPage
    {
        private readonly IFiadoService _fiadoService;

        private List<Fiado> _fiadosExibidos = new();

        private RadioButton _rbPendentes = null!;
        private RadioButton _rbQuitados = null!;
        private RadioButton _rbTodos = null!;
        private Label _lblTotalPendente = null!;
        private DataGridView _gridFiados = null!;

        public FiadosTabPage(IFiadoService fiadoService)
            : base("Fiados")
        {
            _fiadoService = fiadoService;

            ConstruirLayout();
            CarregarFiados();
        }

        private void ConstruirLayout()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ---- Filtro + total pendente ----
            var grpFiltro = new GroupBox { Text = "Filtro", Dock = DockStyle.Fill, Height = 70 };
            var painelFiltro = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };

            _rbPendentes = new RadioButton { Text = "Pendentes", Checked = true, AutoSize = true, Margin = new Padding(0, 6, 20, 0) };
            _rbQuitados = new RadioButton { Text = "Quitados", AutoSize = true, Margin = new Padding(0, 6, 20, 0) };
            _rbTodos = new RadioButton { Text = "Todos", AutoSize = true, Margin = new Padding(0, 6, 20, 0) };
            _rbPendentes.CheckedChanged += (s, e) => { if (_rbPendentes.Checked) CarregarFiados(); };
            _rbQuitados.CheckedChanged += (s, e) => { if (_rbQuitados.Checked) CarregarFiados(); };
            _rbTodos.CheckedChanged += (s, e) => { if (_rbTodos.Checked) CarregarFiados(); };

            _lblTotalPendente = new Label
            {
                Text = "Total pendente: —",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(178, 58, 52),
                Margin = new Padding(30, 6, 0, 0)
            };

            painelFiltro.Controls.Add(_rbPendentes);
            painelFiltro.Controls.Add(_rbQuitados);
            painelFiltro.Controls.Add(_rbTodos);
            painelFiltro.Controls.Add(_lblTotalPendente);
            grpFiltro.Controls.Add(painelFiltro);

            // ---- Grid ----
            var grpLista = new GroupBox { Text = "Fiados", Dock = DockStyle.Fill };
            _gridFiados = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };
            _gridFiados.Columns.Add("Cliente", "Cliente");
            _gridFiados.Columns.Add("DataCriacao", "Data");
            _gridFiados.Columns.Add("Valor", "Valor");
            _gridFiados.Columns.Add("Status", "Status");
            _gridFiados.Columns.Add("DataPagamento", "Pago em");
            grpLista.Controls.Add(_gridFiados);

            // ---- Botões ----
            var painelBotoes = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };

            var btnAtualizar = new Button { Text = "Atualizar lista", Width = 130, Height = 34 };
            var btnReceber = new Button
            {
                Text = "Receber pagamento",
                Width = 160,
                Height = 34,
                BackColor = Color.FromArgb(35, 52, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnExcluir = new Button { Text = "Excluir fiado", Width = 130, Height = 34, ForeColor = Color.FromArgb(178, 58, 52) };

            btnAtualizar.Click += (s, e) => CarregarFiados();
            btnReceber.Click += BtnReceberPagamento_Click;
            btnExcluir.Click += BtnExcluirFiado_Click;

            painelBotoes.Controls.Add(btnAtualizar);
            painelBotoes.Controls.Add(btnReceber);
            painelBotoes.Controls.Add(btnExcluir);

            layout.Controls.Add(grpFiltro, 0, 0);
            layout.Controls.Add(grpLista, 0, 1);
            layout.Controls.Add(painelBotoes, 0, 2);
            Controls.Add(layout);
        }

        /// <summary>
        /// Recarrega a lista de acordo com o filtro selecionado e o total pendente.
        /// Pode ser chamado pelo MainForm sempre que esta aba for selecionada.
        /// </summary>
        public void CarregarFiados()
        {
            _fiadosExibidos = _rbPendentes.Checked
                ? _fiadoService.ListarFiadosPendentes()
                : _rbQuitados.Checked
                    ? _fiadoService.ListarFiadosPagos()
                    : _fiadoService.ListarFiados();

            RenderGridFiados();

            _lblTotalPendente.Text = $"Total pendente: {Formatador.Moeda(_fiadoService.ObterTotalPendente())}";
        }

        private void RenderGridFiados()
        {
            _gridFiados.Rows.Clear();
            foreach (var fiado in _fiadosExibidos)
            {
                _gridFiados.Rows.Add(
                    fiado.Cliente,
                    fiado.DataCriacao.ToString("dd/MM/yyyy HH:mm"),
                    Formatador.Moeda(fiado.ValorTotal),
                    fiado.Pago ? "Pago" : "Aberto",
                    fiado.DataPagamento?.ToString("dd/MM/yyyy HH:mm") ?? "—");
            }
        }

        private Fiado? ObterFiadoSelecionado()
        {
            if (_gridFiados.CurrentRow is null || _gridFiados.CurrentRow.Index < 0)
                return null;

            int index = _gridFiados.CurrentRow.Index;
            if (index >= _fiadosExibidos.Count)
                return null;

            return _fiadosExibidos[index];
        }

        private void BtnReceberPagamento_Click(object? sender, EventArgs e)
        {
            var fiado = ObterFiadoSelecionado();
            if (fiado is null)
            {
                MessageBox.Show(FindForm(), "Selecione um fiado na lista.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (fiado.Pago)
            {
                MessageBox.Show(FindForm(), "Este fiado já foi quitado.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ReceberFiadoDialog(fiado.Cliente, fiado.ValorTotal);
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                return;

            try
            {
                _fiadoService.ReceberPagamento(fiado.Id, dialog.ValorRecebido, dialog.FormaPagamento);
                CarregarFiados();

                MessageBox.Show(FindForm(),
                    $"Fiado de {fiado.Cliente} quitado com sucesso.",
                    "Pagamento recebido", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(FindForm(), ex.Message, "Não foi possível quitar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExcluirFiado_Click(object? sender, EventArgs e)
        {
            var fiado = ObterFiadoSelecionado();
            if (fiado is null)
            {
                MessageBox.Show(FindForm(), "Selecione um fiado na lista.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var resultado = MessageBox.Show(FindForm(),
                $"Excluir o fiado de \"{fiado.Cliente}\" ({Formatador.Moeda(fiado.ValorTotal)})? Essa ação não pode ser desfeita.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
                return;

            try
            {
                _fiadoService.ExcluirFiado(fiado.Id);
                CarregarFiados();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(FindForm(), ex.Message, "Não foi possível excluir",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
