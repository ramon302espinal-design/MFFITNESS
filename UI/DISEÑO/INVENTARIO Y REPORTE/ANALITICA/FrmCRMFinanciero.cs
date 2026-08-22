using System;
using System.Drawing;
using System.Windows.Forms;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Shell visual del CRM Financiero (FASE 1).
    /// Solo estructura UI: Sidebar + Header + Content + Footer.
    /// Sin BLL/DAL/SQL. Hospeda Forms existentes en pnlContent.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCRMFinanciero : Form
    {
        private Form? _vistaActual;
        private Button? _botonActivo;

        public FrmCRMFinanciero()
        {
            InitializeComponent();
            // Design System CRM (FASE 2.3): no aplicar ThemeApplier redondeado del POS.
            CrmVisualTokens.MarkClassic(this);
        }

        private void FrmCRMFinanciero_Load(object sender, EventArgs e)
        {
            MostrarVista(() => new FrmAnaDashboard(), btnDashboard, "Dashboard financiero",
                "Vision general del rendimiento financiero");
        }

        private void btnDashboard_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaDashboard(), btnDashboard, "Dashboard financiero",
                "Vision general del rendimiento financiero");

        private void btnInversiones_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaInversiones(), btnInversiones, "Inversiones",
                "Capital invertido y seguimiento de inversiones");

        private void btnRentabilidad_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaliticaInventario(), btnRentabilidad, "Rentabilidad / Inventario",
                "Valor, margen, rotacion y capital inmovilizado");

        private void btnRanking_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaRanking(), btnRanking, "Ranking",
                "Unidades, ingresos, ganancia, margen, ROI y rotacion (una metrica)");

        private void btnVentas_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaVentas(), btnVentas, "Ventas",
                "Volumen, unidades y rendimiento comercial");

        private void btnGanancias_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaGanancias(), btnGanancias, "Ganancias",
                "Ganancia realizada y potencial");

        private void btnROI_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaRoi(), btnROI, "ROI",
                "Retorno de inversion general y por producto");

        private void btnCapital_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaCapitalCongelado(), btnCapital, "Capital congelado",
                "Capital inmovilizado clasificado (Frozen + Critical)");

        private void btnAlertas_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaAlertas(), btnAlertas, "Alertas",
                "Avisos de capital, inventario y riesgo");

        private void btnTendencias_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaTendencias(), btnTendencias, "Tendencias",
                "MoM por producto (creciendo / estable / cayendo)");

        /// <summary>Navegación pública al Centro (FASE 10.28 — Dashboard Ver/Analizar).</summary>
        public void MostrarDecisiones()
            => MostrarVista(() => new FrmAnaDecisiones(), btnDecisiones, "Centro de decisiones",
                "Prioridades de hoy · detectar / analizar / recomendar — usted decide");

        public void MostrarCapitalCongelado()
            => MostrarVista(() => new FrmAnaCapitalCongelado(), btnCapital, "Capital congelado",
                "Capital inmovilizado clasificado (Frozen + Critical)");

        public void MostrarAlertas()
            => MostrarVista(() => new FrmAnaAlertas(), btnAlertas, "Alertas",
                "Avisos de capital, inventario y riesgo");

        public void MostrarProductosEstrella()
            => MostrarVista(() => new FrmAnaProductosEstrella(), btnConfiguracion, "Productos estrella",
                "Impacto + eficiencia + bajo riesgo (explicable)");

        public void MostrarTendencias()
            => MostrarVista(() => new FrmAnaTendencias(), btnTendencias, "Tendencias",
                "MoM por producto (creciendo / estable / cayendo)");

        private void btnDecisiones_Click(object sender, EventArgs e)
            => MostrarDecisiones();

        /// <summary>
        /// Hospeda FrmReportes (POS) en el shell. No modifica su lógica:
        /// solo oculta la barra de navegación duplicada al embeber.
        /// </summary>
        private void btnReportesPos_Click(object sender, EventArgs e)
            => MostrarVista(CrearVistaReportesPos, btnReportesPos, "Reportes POS",
                "Reportes operativos del sistema (legado)");

        private static Form CrearVistaReportesPos()
        {
            var vista = new FrmReportes();
            PrepararReportesEmbebido(vista);
            return vista;
        }

        /// <summary>
        /// Ajuste solo de presentación al embeber: oculta panelNav del POS
        /// y revierte el offset vertical que FrmReportes aplica para esa barra.
        /// Sin cambios en generación/exportación de reportes.
        /// </summary>
        private static void PrepararReportesEmbebido(FrmReportes vista)
        {
            const int offsetNav = 52;
            Control? nav = null;
            foreach (Control c in vista.Controls)
            {
                if (string.Equals(c.Name, "panelNav", StringComparison.Ordinal))
                {
                    nav = c;
                    break;
                }
            }

            if (nav == null)
                return;

            nav.Visible = false;
            foreach (Control c in vista.Controls)
            {
                if (c.Dock != DockStyle.None)
                    continue;
                c.Top = Math.Max(0, c.Top - offsetNav);
            }
        }

        private void btnEstrellas_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaProductosEstrella(), btnConfiguracion, "Productos estrella",
                "Impacto + eficiencia + bajo riesgo (explicable)");

        /// <summary>
        /// Solo hospeda la vista hija en pnlContent. Sin logica de negocio.
        /// </summary>
        private void MostrarVista(Func<Form> factory, Button boton, string titulo, string subtitulo)
        {
            if (_vistaActual != null)
            {
                pnlContent.Controls.Remove(_vistaActual);
                _vistaActual.Dispose();
                _vistaActual = null;
            }

            Form vista = factory();
            vista.TopLevel = false;
            vista.FormBorderStyle = FormBorderStyle.None;
            vista.Dock = DockStyle.Fill;
            CrmVisualTokens.MarkClassic(vista);
            pnlContent.Controls.Add(vista);
            vista.Show();
            _vistaActual = vista;

            lblTitle.Text = titulo;
            lblSubtitle.Text = subtitulo;
            MarcarBotonActivo(boton);
        }

        private void MarcarBotonActivo(Button boton)
        {
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Button b && b.Name.StartsWith("btn", StringComparison.Ordinal))
                {
                    b.BackColor = CrmVisualTokens.NavIdleBg;
                    b.ForeColor = CrmVisualTokens.NavIdleFg;
                    b.FlatAppearance.BorderSize = 0;
                }
            }

            boton.BackColor = CrmVisualTokens.Border;
            boton.ForeColor = CrmVisualTokens.NavActiveFg;
            _botonActivo = boton;
        }
    }
}
