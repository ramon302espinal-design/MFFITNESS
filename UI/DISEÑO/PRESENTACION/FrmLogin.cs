using BLL;
using DTO;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CORE;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmLogin : Form
    {
        private readonly UsuarioBLL usuarioBLL = new UsuarioBLL();

        public FrmLogin()
        {
            InitializeComponent();
            ThemeHost.Attach(this, ApplyLoginTheme);
            ConfigurarUI();
        }

        private void ApplyLoginTheme()
        {
            BackColor = AppTheme.Secondary;
            tableLayoutPanel1.BackColor = AppTheme.Secondary;
            panelCard.BackColor = AppTheme.Surface;
            panelCard.Tag = "card";
            panelHeader.BackColor = AppTheme.Primary;
            panelFormulario.BackColor = AppTheme.Surface;
            panelBotones.BackColor = AppTheme.Surface;

            lblMF.ForeColor = AppTheme.TextOnPrimary;
            lblFitness.ForeColor = AppTheme.TextOnPrimary;
            
            lblUsuario.ForeColor = AppTheme.TextPrimary;
            lblContraseña.ForeColor = AppTheme.TextPrimary;
            lblVersion.ForeColor = AppTheme.TextMuted;

            var logo = ShellTheme.LoadLogo();
            if (logo != null)
            {
                lblMF.Text = "";
                lblFitness.Text = "";
                var previous = picLoginLogo.Image;
                picLoginLogo.SizeMode = PictureBoxSizeMode.Zoom;
                picLoginLogo.Image = logo;
                previous?.Dispose();
            }

            ThemeApplier.StyleTextBox(txtContraseña);
            ThemeApplier.StyleComboBox(comboUsuarios);
            ThemeApplier.StyleCheckBox(chkMostrarContraseña);
            ThemeApplier.StyleButton(btnIniciar, ButtonVariant.Primary);
            ThemeApplier.StyleButton(btnCancelar, ButtonVariant.Secondary);
            ThemeApplier.ApplyRoundedRegion(btnIniciar, AppTheme.RadiusMedium);
            ThemeApplier.ApplyRoundedRegion(btnCancelar, AppTheme.RadiusMedium);
        }


        // ===============================
        // LIMPIAR CONTROLES LOGOUT
        // ===============================
        private void LimpiarControles()
        {
            // Selecciona el primer usuario del combo al limpiar
            if (comboUsuarios.Items.Count > 0)
                comboUsuarios.SelectedIndex = 0;

            txtContraseña.Clear();
            chkMostrarContraseña.Checked = false;
            ActualizarVisibilidadContraseña();
            txtContraseña.Focus(); // cursor en contraseña
        }

        /// <summary>
        /// Con PasswordChar custom, UseSystemPasswordChar solo no revela el texto:
        /// hay que poner PasswordChar = '\0' para mostrar.
        /// </summary>
        private void ActualizarVisibilidadContraseña()
        {
            bool mostrar = chkMostrarContraseña.Checked;
            txtContraseña.UseSystemPasswordChar = false;
            txtContraseña.PasswordChar = mostrar ? '\0' : '●';
        }

        private void chkMostrarContraseña_CheckedChanged(object? sender, EventArgs e)
        {
            ActualizarVisibilidadContraseña();
        }
        private void ConfigurarUI()
        {
            this.AcceptButton = btnIniciar;
            this.ActiveControl = comboUsuarios;
            lblVersion.Text =
                $"v{AppVersion.SemanticVersion} · {AppConfig.EnvironmentName} · {AppConfig.DatabaseName}";
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            chkMostrarContraseña.Checked = false;
            ActualizarVisibilidadContraseña();
        }





        private void Login()
        {
            try
            {
                if (comboUsuarios.SelectedValue == null || comboUsuarios.SelectedIndex < 0)
                {
                    MessageBox.Show(
                        "No hay usuario seleccionado. Verifique que existan usuarios activos en la base.",
                        "Validación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                int idUsuario = Convert.ToInt32(comboUsuarios.SelectedValue);
                string clave = txtContraseña.Text.Trim();

                if (string.IsNullOrWhiteSpace(clave))
                {
                    MessageBox.Show("Ingrese la contraseña", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnIniciar.Enabled = false;
                Cursor = Cursors.WaitCursor;

                UsuarioDTO user = usuarioBLL.LoginPorId(idUsuario, clave);

                if (user != null)
                {
                    Sesion.Iniciar(
                         user.Id,
                         user.Usuario ?? string.Empty,
                         user.RolId,
                         user.Rol ?? string.Empty,
                         user.Permisos
                     );

                    AbrirSistema();
                }
                else
                {
                    MessageBox.Show("Contraseña incorrecta", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtContraseña.Clear();
                    txtContraseña.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar sesión:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnIniciar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void AbrirSistema()
        {
            FrmPresentacion frm = new FrmPresentacion();
            frm.Show();       // mostramos el principal
            this.Hide();       // ocultamos login
        }
        private void FrmLogin_Load(object sender, EventArgs e)
        {
            ShellTheme.TryApplyFormIcon(this);
            chkMostrarContraseña.Checked = false;
            ActualizarVisibilidadContraseña();

            DataTable dtUsuarios = usuarioBLL.TraerUsuariosActivos();
            comboUsuarios.DataSource = dtUsuarios;
            comboUsuarios.DisplayMember = "Usuario";
            comboUsuarios.ValueMember = "Id";

            // Solo seleccionar si hay filas (BD vacía / sin Activo=1 → Items.Count = 0).
            if (comboUsuarios.Items.Count > 0)
                comboUsuarios.SelectedIndex = 0;
            else
            {
                comboUsuarios.SelectedIndex = -1;
                btnIniciar.Enabled = false;
                MessageBox.Show(
                    "No hay usuarios activos en esta base de datos.\n\n" +
                    $"BD: {AppConfig.DatabaseName} ({AppConfig.EnvironmentName})\n\n" +
                    "Si estás en Development (MF_CYBER_DB_DEV), clona usuarios desde producción " +
                    "o cambia a Production ([MF CYBER DB]).",
                    "Sin usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            comboUsuarios.Format += (s, ev) =>
            {
                if (ev.ListItem is DataRowView drv)
                    ev.Value = drv["Usuario"]?.ToString()?.ToUpperInvariant();
            };
        }



        private void btnEntrar_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FrmLogin_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible) // solo cuando el form se vuelve visible
            {
                LimpiarControles();
            }
        }
    }
}