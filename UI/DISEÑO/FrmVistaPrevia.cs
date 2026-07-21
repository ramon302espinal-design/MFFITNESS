using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmVistaPrevia : Form
    {
        private string contenido = "";
        private PrintDocument printDocument;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmVistaPrevia()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        public FrmVistaPrevia(string texto) : this()
        {
            contenido = texto ?? "";
        }

        private void FrmVistaPrevia_Load(object sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;
            txtVistaPrevia.Text = contenido;
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog
                {
                    Document = printDocument
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                    MessageBox.Show("Documento enviado a la impresora", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null)
                return;

            Font font = new Font("Courier New", 9);
            float yPos = e.MarginBounds.Top;
            int count = 0;
            float leftMargin = e.MarginBounds.Left;
            float linesPerPage = e.MarginBounds.Height / font.GetHeight(e.Graphics);

            string[] lines = contenido.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (count < linesPerPage)
                {
                    yPos = e.MarginBounds.Top + (count * font.GetHeight(e.Graphics));
                    e.Graphics.DrawString(line, font, Brushes.Black, leftMargin, yPos, new StringFormat());
                    count++;
                }
            }

            e.HasMorePages = (count < lines.Length);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
