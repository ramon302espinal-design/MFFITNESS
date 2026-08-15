using BLL;
using CORE;
using System;
using System.Windows.Forms;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmDeudaDashboard : Form
    {
        private DeudaBLL deudaBLL = new DeudaBLL();

        public FrmDeudaDashboard()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        private void FrmDeudaDashboard_Load(object sender, EventArgs e)
        {
            CargarTodo();

            AppEventos.OnPagoRegistrado += ActualizarDashboard;
            AppEventos.OnDeudaModificada += ActualizarDashboard;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= ActualizarDashboard;
            AppEventos.OnDeudaModificada -= ActualizarDashboard;
            base.OnFormClosed(e);
        }

        private void CargarTodo()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            try
            {
                lblDeudasActivas.Text = deudaBLL.DeudasActivas().ToString();
                lblDeudasVencidas.Text = deudaBLL.DeudasVencidas().ToString();
                lblIngresoPendiente.Text = deudaBLL.IngresoPendiente().ToString("N2");
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando dashboard: " + ex.Message);
            }
        }

        private void ActualizarDashboard()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(ActualizarDashboard));
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            CargarTodo();
        }

        /// <summary>
        /// Refresco público desde el módulo principal.
        /// </summary>
        public void ActualizarDatos()
        {
            CargarTodo();
        }
    }
}
