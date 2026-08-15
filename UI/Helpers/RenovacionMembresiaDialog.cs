using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using BLL.Models;
using CORE;
using UI.Facturas;

namespace UI.Helpers
{
    public static class RenovacionMembresiaDialog
    {
        public static bool Mostrar(
            IWin32Window? owner,
            int clienteId,
            string nombreCliente,
            Action? onRenovacionExitosa = null)
        {
            var deudaBLL = new DeudaBLL();
            if (AvisoDeudaPendiente.BloqueaOperacionDePlan(owner, clienteId, deudaBLL))
                return false;

            using Form frm = new Form
            {
                Text = "Renovar - " + nombreCliente,
                Size = new Size(350, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            Label lblPrecio = new Label
            {
                Location = new Point(30, 70),
                AutoSize = true,
                Text = "Precio: RD$ 0.00"
            };

            PlanBLL planBLL = new PlanBLL();
            DataTable tablaPlanes = planBLL.ObtenerPlanes() ?? new DataTable();
            DataTable fuenteCombo = FiltrarPlanesRenovacion(tablaPlanes);

            ComboBox comboPlanes = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(30, 30),
                Width = 250,
                DisplayMember = "Nombre",
                ValueMember = "Id"
            };

            if (fuenteCombo.Rows.Count > 0)
            {
                comboPlanes.DataSource = fuenteCombo;
                comboPlanes.SelectedIndex = -1;
            }

            comboPlanes.SelectedIndexChanged += (_, _) =>
            {
                if (comboPlanes.SelectedItem is DataRowView row &&
                    row["Precio"] != DBNull.Value)
                {
                    decimal precio = Convert.ToDecimal(row["Precio"]);
                    lblPrecio.Text = "Precio: RD$ " + precio.ToString("0.00");
                }
            };

            Button btnConfirmar = new Button
            {
                Text = "CONFIRMAR",
                Location = new Point(30, 120),
                Width = 120,
                Height = 30
            };

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Location = new Point(160, 120),
                Width = 120,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };

            bool renovacionCompletada = false;

            btnConfirmar.Click += (_, _) =>
            {
                try
                {
                    if (comboPlanes.SelectedValue == null || comboPlanes.SelectedIndex < 0)
                    {
                        MessageBox.Show(frm, "Seleccione un plan.", "Renovación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int planId = Convert.ToInt32(comboPlanes.SelectedValue);
                    var plan = planBLL.ObtenerPlan(planId);

                    if (plan == null)
                    {
                        MessageBox.Show(frm, "El plan no existe o no se pudo cargar.", "Renovación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string usuario = Sesion.Usuario ?? "ADMIN";
                    CajaBLL cajaBLL = new CajaBLL();

                    if (cajaBLL.ObtenerCajaAbiertaHoy() == null)
                    {
                        DialogResult r = MessageBox.Show(
                            frm,
                            "No hay caja abierta. ¿Deseas abrir caja?",
                            "Caja",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (r != DialogResult.Yes)
                            return;

                        string input = Microsoft.VisualBasic.Interaction.InputBox(
                            "Ingrese monto inicial:",
                            "Abrir Caja",
                            "0");

                        if (!decimal.TryParse(input, out decimal montoInicial) || montoInicial <= 0)
                        {
                            MessageBox.Show(frm, "Monto inválido.", "Caja",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        cajaBLL.AbrirCaja(montoInicial, usuario);
                        AppEventos.CajaCambiada();
                    }

                    btnConfirmar.Enabled = false;
                    btnCancelar.Enabled = false;
                    frm.Cursor = Cursors.WaitCursor;

                    var result = MembresiaCommandService.RenovarMembresia(
                        clienteId,
                        planId,
                        plan.Precio,
                        usuario);

                    if (!result.Success)
                    {
                        MessageBox.Show(frm, result.Message, "Renovación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Capturar datos; PDF/WhatsApp fuera del modal (evita freeze al confirmar).
                    if (result.Payload is RenovacionOperacionResult opRen)
                    {
                        int pagoIdBg = opRen.PagoId;
                        int membresiaIdBg = opRen.MembresiaId;
                        int cajaMovIdBg = opRen.CajaMovimientoId;
                        DateTime finBg = opRen.FechaFinMembresia == default
                            ? MembresiaHelper.CalcularFechaVencimiento(DateTime.Now)
                            : opRen.FechaFinMembresia;
                        string planNombreBg = plan.Nombre ?? "PLAN";
                        decimal precioBg = plan.Precio;

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                FacturaMembresiaPdfService.GenerarDesdeOperacion(
                                    owner: null,
                                    clienteId,
                                    planNombreBg,
                                    precioBg,
                                    finBg,
                                    "Efectivo",
                                    new MembresiaOperacionResult
                                    {
                                        MembresiaId = membresiaIdBg,
                                        PagoId = pagoIdBg,
                                        CajaMovimientoId = cajaMovIdBg,
                                        FechaFinMembresia = finBg
                                    },
                                    abrirPdf: false);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[PDF renovación] {ex.Message}");
                            }
                        });
                    }

                    renovacionCompletada = true;
                    frm.DialogResult = DialogResult.OK;
                    frm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(frm, "Error: " + ex.Message, "Renovación",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (!frm.IsDisposed)
                    {
                        frm.Cursor = Cursors.Default;
                        btnConfirmar.Enabled = true;
                        btnCancelar.Enabled = true;
                    }
                }
            };

            frm.Controls.Add(comboPlanes);
            frm.Controls.Add(lblPrecio);
            frm.Controls.Add(btnConfirmar);
            frm.Controls.Add(btnCancelar);
            frm.CancelButton = btnCancelar;
            frm.AcceptButton = btnConfirmar;

            frm.ShowDialog(owner);

            // Refresco del grid/dashboard solo cuando el modal ya cerró (evita deadlock UI).
            if (renovacionCompletada)
                onRenovacionExitosa?.Invoke();

            return renovacionCompletada;
        }

        private static DataTable FiltrarPlanesRenovacion(DataTable planes)
        {
            if (planes.Rows.Count == 0 || !planes.Columns.Contains("Nombre"))
                return planes;

            DataView dv = planes.DefaultView;
            try
            {
                dv.RowFilter = "Nombre IN ('PREMIUM', 'PRO', '3x', 'MENSUALIDAD')";
                DataTable filtrada = dv.ToTable();
                return filtrada.Rows.Count > 0 ? filtrada : planes.Copy();
            }
            catch
            {
                return planes.Copy();
            }
            finally
            {
                dv.RowFilter = string.Empty;
            }
        }
    }
}
