using CaixaFestejos.Data;
using System;
using System.Windows.Forms;

namespace CaixaFestejos
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var database = new Database();
                var initializer = new DatabaseInitializer(database);
                initializer.Inicializar();

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}