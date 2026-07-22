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

            var logo = ShellTheme.LoadLogo();
            if (logo != null)
            {
                lblMF.Text = "";
                lblFitness.Text = "";
                picLoginLogo.Image = logo;
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
            txtContraseña.Focus(); // cursor en contraseña
        }
        private void ConfigurarUI()
        {
            txtContraseña.UseSystemPasswordChar = true;
            this.AcceptButton = btnIniciar;
            this.ActiveControl = comboUsuarios;
        }





        private void Login()
        {
            try
            {
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
            txtContraseña.UseSystemPasswordChar = true;
            var dtUsuarios = usuarioBLL.TraerUsuariosActivos(); // devuelve DataTable con Id, Usuario, Rol
            comboUsuarios.DataSource = dtUsuarios;
            comboUsuarios.DisplayMember = "Usuario"; // lo que se ve
            comboUsuarios.ValueMember = "Id";        // valor interno
            comboUsuarios.SelectedIndex = 0;         // seleccionamos el primero

            // 🔹 Formateamos el texto a mayúsculas
            comboUsuarios.Format += (s, ev) =>
            {
                if (ev.ListItem != null)
                {
                    DataRowView drv = ev.ListItem as DataRowView;
                    if (drv != null)
                        ev.Value = drv["Usuario"].ToString()?.ToUpper(); // todo en mayúscula



                }
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

        private void chkMostrarContraseña_CheckedChanged(object sender, EventArgs e)
        {
            // Si el checkbox está marcado, mostramos la contraseña
            txtContraseña.UseSystemPasswordChar = !chkMostrarContraseña.Checked;
        }
    }
}