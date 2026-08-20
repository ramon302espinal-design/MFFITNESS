using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UI.Theme;

namespace UI.DISEÑO.ANALITICA.Controles
{
    /// <summary>
    /// Contenedor visual reutilizable para secciones del CRM Financiero.
    /// Solo presenta UI: sin SQL, BLL ni cálculos financieros.
    /// </summary>
    public partial class UcCrmSectionPanel : UserControl
    {
        private bool _mostrarHeader = true;
        private bool _mostrarAcciones;
        private CrmVisualState _estadoVisual = CrmVisualState.Normal;

        public UcCrmSectionPanel()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            AplicarEstadoVisual();
            AplicarVisibilidadHeader();
            AplicarVisibilidadAcciones();
        }

        /// <summary>Título mostrado en el encabezado de la sección.</summary>
        [Category("CRM")]
        [Description("Texto del encabezado de la sección.")]
        [DefaultValue("Sección")]
        public string Titulo
        {
            get => lblSectionTitle.Text;
            set => lblSectionTitle.Text = value ?? string.Empty;
        }

        /// <summary>Muestra u oculta la franja de encabezado.</summary>
        [Category("CRM")]
        [Description("Si es false, solo queda el cuerpo de la sección.")]
        [DefaultValue(true)]
        public bool MostrarHeader
        {
            get => _mostrarHeader;
            set
            {
                if (_mostrarHeader == value)
                    return;
                _mostrarHeader = value;
                AplicarVisibilidadHeader();
            }
        }

        /// <summary>Muestra u oculta el área de botones a la derecha del título.</summary>
        [Category("CRM")]
        [Description("Área FlowLayoutPanel para botones de acción del encabezado.")]
        [DefaultValue(false)]
        public bool MostrarAcciones
        {
            get => _mostrarAcciones;
            set
            {
                if (_mostrarAcciones == value)
                    return;
                _mostrarAcciones = value;
                AplicarVisibilidadAcciones();
            }
        }

        /// <summary>Estado visual (solo color de borde/header; sin lógica de negocio).</summary>
        [Category("CRM")]
        [Description("Apariencia visual de la sección. No implica reglas financieras.")]
        [DefaultValue(CrmVisualState.Normal)]
        public CrmVisualState EstadoVisual
        {
            get => _estadoVisual;
            set
            {
                if (_estadoVisual == value)
                    return;
                _estadoVisual = value;
                AplicarEstadoVisual();
            }
        }

        /// <summary>Panel donde se colocan KPIs, grids u otros controles hijos.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel Cuerpo => pnlSectionBody;

        /// <summary>Contenedor de acciones del encabezado (visible si MostrarAcciones = true).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public FlowLayoutPanel Acciones => flpSectionActions;

        private void AplicarVisibilidadHeader()
        {
            pnlSectionHeader.Visible = _mostrarHeader;
            if (!_mostrarHeader)
                pnlSectionHeader.Height = 0;
            else if (pnlSectionHeader.Height < CrmVisualTokens.HeightHeaderSection - 4)
                pnlSectionHeader.Height = CrmVisualTokens.HeightHeaderSection;
        }

        private void AplicarVisibilidadAcciones()
        {
            flpSectionActions.Visible = _mostrarAcciones;
        }

        private void AplicarEstadoVisual()
        {
            pnlRoot.BackColor = CrmVisualTokens.BorderForState(_estadoVisual);
            pnlSectionHeader.BackColor = CrmVisualTokens.HeaderBgForState(_estadoVisual);
            pnlSectionBody.BackColor = CrmVisualTokens.Surface;
        }
    }
}
