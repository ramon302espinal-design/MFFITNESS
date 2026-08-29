using System;
using System.Drawing;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Shell visual del CRM Financiero (FASE 1).
    /// Sidebar + Header (título / período auto) + Content + Footer.
    /// Hospeda Forms existentes en pnlContent.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmCRMFinanciero : Form
    {
        private Form? _vistaActual;
        private Button? _botonActivo;
        private bool _suppressPeriodRefresh;

        public FrmCRMFinanciero()
        {
            InitializeComponent();
            // Design System CRM (FASE 2.3): no aplicar ThemeApplier redondeado del POS.
            CrmVisualTokens.MarkClassic(this);
            if (!ThemeHost.IsDesignTime())
                ModuloAtajosTeclado.WireAtajosEnFormulario(this);
        }

        private void FrmCRMFinanciero_Load(object sender, EventArgs e)
        {
            _suppressPeriodRefresh = true;
            dtDesdePeriodo.Value = DateTime.Today.AddDays(-29);
            dtHastaPeriodo.Value = DateTime.Today;

            // Default: Este mes (índice 5 en el combo ampliado).
            if (cmbPeriodo.Items.Count > 5)
                cmbPeriodo.SelectedIndex = 5;
            else if (cmbPeriodo.Items.Count > 0 && cmbPeriodo.SelectedIndex < 0)
                cmbPeriodo.SelectedIndex = 0;

            cmbPeriodo.Enabled = true;
            ActualizarUiRangoPersonalizado();
            panelHeader.Visible = true;
            panelHeader.BringToFront();

            MostrarVista(() => CrearDashboard(), btnDashboard,
                "Dashboard financiero",
                "Vision general del rendimiento financiero");
            _suppressPeriodRefresh = false;
        }

        private FrmAnaDashboard CrearDashboard()
        {
            var dash = new FrmAnaDashboard(PeriodoSeleccionado());
            if (EsPersonalizado()
                && TryObtenerRangoPersonalizado(out DateTime desde, out DateTime hastaExcl))
            {
                dash.Recargar(ProfitPeriodKind.Custom, desde, hastaExcl);
            }

            return dash;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
            => MostrarVista(() => CrearDashboard(), btnDashboard,
                "Dashboard financiero",
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
            vista.PrepararParaEmbebido();
            return vista;
        }

        private void btnEstrellas_Click(object sender, EventArgs e)
            => MostrarVista(() => new FrmAnaProductosEstrella(), btnConfiguracion, "Productos estrella",
                "Impacto + eficiencia + bajo riesgo (explicable)");

        private void cmbPeriodo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarUiRangoPersonalizado();
            if (_suppressPeriodRefresh)
                return;
            RefrescarVistaActual();
        }

        private void PeriodoFechas_ValueChanged(object? sender, EventArgs e)
        {
            ActualizarEtiquetaDias();
            if (_suppressPeriodRefresh || !EsPersonalizado())
                return;
            RefrescarVistaActual();
        }

        private bool EsPersonalizado()
            => cmbPeriodo.SelectedIndex == 8
               || string.Equals(cmbPeriodo.SelectedItem?.ToString(), "Personalizado", StringComparison.OrdinalIgnoreCase);

        private void ActualizarUiRangoPersonalizado()
        {
            bool personalizado = EsPersonalizado();
            dtDesdePeriodo.Visible = personalizado;
            dtHastaPeriodo.Visible = personalizado;
            lblDiasPeriodo.Visible = personalizado;
            if (personalizado)
                ActualizarEtiquetaDias();
        }

        private void ActualizarEtiquetaDias()
        {
            DateTime desde = dtDesdePeriodo.Value.Date;
            DateTime hasta = dtHastaPeriodo.Value.Date;
            if (desde > hasta)
            {
                lblDiasPeriodo.ForeColor = Color.FromArgb(220, 38, 38);
                lblDiasPeriodo.Text = "Rango invalido";
                return;
            }

            // Inclusive: del 10 al 10 = 1 día; del 10 al 12 = 3 días.
            int dias = (hasta - desde).Days + 1;
            lblDiasPeriodo.ForeColor = Color.FromArgb(37, 99, 235);
            lblDiasPeriodo.Text = dias == 1 ? "1 dia" : $"{dias} dias";
        }

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
            // PrepararParaEmbebido de Reportes ya fija TopLevel/Dock; el resto de vistas CRM igual.
            if (vista.TopLevel)
            {
                vista.TopLevel = false;
                vista.FormBorderStyle = FormBorderStyle.None;
                vista.WindowState = FormWindowState.Normal;
                vista.MinimumSize = Size.Empty;
                vista.Dock = DockStyle.Fill;
            }

            CrmVisualTokens.MarkClassic(vista);
            pnlContent.Controls.Add(vista);
            vista.Show();
            vista.BringToFront();
            vista.PerformLayout();
            pnlContent.PerformLayout();
            _vistaActual = vista;

            // Sincroniza período del panelHeader del shell con la vista (Reportes POS, dashboards…).
            if (vista is ICrmPeriodRefreshable refreshable)
            {
                if (EsPersonalizado()
                    && TryObtenerRangoPersonalizado(out DateTime desde, out DateTime hastaExcl))
                {
                    refreshable.Recargar(ProfitPeriodKind.Custom, desde, hastaExcl);
                }
                else
                {
                    refreshable.Recargar(PeriodoSeleccionado());
                }
            }

            lblTitle.Text = titulo;
            lblSubtitle.Text = subtitulo;
            panelHeader.Visible = true;
            panelHeader.BringToFront();
            MarcarBotonActivo(boton);
        }

        private void RefrescarVistaActual()
        {
            if (_vistaActual is ICrmPeriodRefreshable refreshable)
            {
                if (EsPersonalizado()
                    && TryObtenerRangoPersonalizado(out DateTime desde, out DateTime hastaExcl))
                {
                    refreshable.Recargar(ProfitPeriodKind.Custom, desde, hastaExcl);
                }
                else if (!EsPersonalizado())
                {
                    refreshable.Recargar(PeriodoSeleccionado());
                }

                return;
            }

            // Vistas sin período: no-op seguro (header sigue visible/usable).
        }

        private bool TryObtenerRangoPersonalizado(out DateTime desde, out DateTime hastaExclusivo)
        {
            desde = dtDesdePeriodo.Value.Date;
            DateTime hasta = dtHastaPeriodo.Value.Date;
            hastaExclusivo = hasta.AddDays(1);
            if (desde > hasta)
                return false;
            return true;
        }

        private ProfitPeriodKind PeriodoSeleccionado()
            => cmbPeriodo.SelectedIndex switch
            {
                0 => ProfitPeriodKind.Today,
                1 => ProfitPeriodKind.Yesterday,
                2 => ProfitPeriodKind.Last7Days,
                3 => ProfitPeriodKind.Last14Days,
                4 => ProfitPeriodKind.Last30Days,
                5 => ProfitPeriodKind.ThisMonth,
                6 => ProfitPeriodKind.ThisQuarter,
                7 => ProfitPeriodKind.ThisYear,
                8 => ProfitPeriodKind.Custom,
                _ => ProfitPeriodKind.ThisMonth
            };

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
