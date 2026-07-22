namespace UI.DISEÑO
{
    partial class FrmClientes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            layoutNavClientes = new TableLayoutPanel();
            panelNav = new Panel();
            btnNavClientes = new Button();
            btnNavReportes = new Button();
            btnNavInventario = new Button();
            btnNavHistorial = new Button();
            btnNavCaja = new Button();
            btnNavEstado = new Button();
            btnNavDeudas = new Button();
            btnNavPagar = new Button();
            btnNavBack = new Button();
            tabControlClientes = new TabControl();
            tabAgregar = new TabPage();
            txtLesionDescripcion = new TextBox();
            cmbsexo = new ComboBox();
            label16 = new Label();
            label15 = new Label();
            chkAsma = new CheckBox();
            chkHipertension = new CheckBox();
            chkDiabetes = new CheckBox();
            label14 = new Label();
            label13 = new Label();
            txtEmergenciaTelefonoAlt = new TextBox();
            txtEmergenciaTelefono = new TextBox();
            txtEmergenciaParentesco = new TextBox();
            txtEmergenciaNombre = new TextBox();
            label12 = new Label();
            label11 = new Label();
            label10 = new Label();
            label9 = new Label();
            label8 = new Label();
            lblEdad = new Label();
            btnAgregar = new Button();
            dtpFechaIngreso = new DateTimePicker();
            label7 = new Label();
            txtDireccion = new TextBox();
            label3 = new Label();
            txtTelefono = new TextBox();
            label4 = new Label();
            txtFecha = new DateTimePicker();
            label5 = new Label();
            txtNombre = new TextBox();
            label1 = new Label();
            txtId = new TextBox();
            label2 = new Label();
            chkProblemasCardiacos = new CheckBox();
            chkColesterolAlto = new CheckBox();
            chkArtritis = new CheckBox();
            chkHernia = new CheckBox();
            chkEpilepsia = new CheckBox();
            chkEmbarazo = new CheckBox();
            chkNingunaEnfermedad = new CheckBox();
            lblEnfermedadOtra = new Label();
            txtEnfermedadOtra = new TextBox();
            lblLesiones = new Label();
            chkLesionHombro = new CheckBox();
            chkLesionRodilla = new CheckBox();
            chkLesionEspalda = new CheckBox();
            chkLesionCuello = new CheckBox();
            chkLesionTobillo = new CheckBox();
            chkLesionCadera = new CheckBox();
            lblLesionDescripcion = new Label();
            lblMedicamentos = new Label();
            panelMedicamentos = new Panel();
            rbMedicamentosSi = new RadioButton();
            rbMedicamentosNo = new RadioButton();
            lblListaMedicamentos = new Label();
            txtListaMedicamentos = new TextBox();
            lblAlergias = new Label();
            panelAlergias = new Panel();
            rbAlergiasSi = new RadioButton();
            rbAlergiasNo = new RadioButton();
            lblAlergiasDescripcion = new Label();
            txtAlergiasDescripcion = new TextBox();
            lblCirugias = new Label();
            panelCirugias = new Panel();
            rbCirugiasSi = new RadioButton();
            rbCirugiasNo = new RadioButton();
            lblCirugiasDescripcion = new Label();
            txtCirugiasDescripcion = new TextBox();
            lblCirugiasFecha = new Label();
            dtpCirugiasFecha = new DateTimePicker();
            lblCirugiaAntiguedad = new Label();
            lblObjetivoFitness = new Label();
            lblObjetivoHint = new Label();
            chkObjPerderGrasa = new CheckBox();
            chkObjGanarMasa = new CheckBox();
            chkObjTonificar = new CheckBox();
            chkObjMejorarCondicion = new CheckBox();
            chkObjRehabilitacion = new CheckBox();
            chkObjSalud = new CheckBox();
            chkObjCompetencia = new CheckBox();
            chkObjOtro = new CheckBox();
            txtObjOtroDescripcion = new TextBox();
            lblExperiencia = new Label();
            panelExperiencia = new Panel();
            rbExpNunca = new RadioButton();
            rbExpMenos6 = new RadioButton();
            rbExp1Ano = new RadioButton();
            rbExp2Anos = new RadioButton();
            rbExpMas5 = new RadioButton();
            lblHorarioPreferido = new Label();
            panelHorario = new Panel();
            rbHorManana = new RadioButton();
            rbHorTarde = new RadioButton();
            rbHorNoche = new RadioButton();
            rbHorVariado = new RadioButton();
            txtHorarioVariadoDetalle = new TextBox();
            tabMiembros = new TabPage();
            layoutMiembros = new TableLayoutPanel();
            panelToolbarMiembros = new Panel();
            label6 = new Label();
            txtBuscar = new TextBox();
            dgvClientes = new DataGridView();
            panelDetalleScroll = new Panel();
            ucFichaResumen = new UI.DISEÑO.Controles.UcFichaResumenMiembro();
            btnBack = new Button();
            layoutNavClientes.SuspendLayout();
            panelNav.SuspendLayout();
            tabControlClientes.SuspendLayout();
            tabAgregar.SuspendLayout();
            panelMedicamentos.SuspendLayout();
            panelAlergias.SuspendLayout();
            panelCirugias.SuspendLayout();
            panelExperiencia.SuspendLayout();
            panelHorario.SuspendLayout();
            tabMiembros.SuspendLayout();
            layoutMiembros.SuspendLayout();
            panelToolbarMiembros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).BeginInit();
            panelDetalleScroll.SuspendLayout();
            SuspendLayout();
            // 
            // layoutNavClientes
            // 
            layoutNavClientes.ColumnCount = 1;
            layoutNavClientes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutNavClientes.Controls.Add(panelNav, 0, 0);
            layoutNavClientes.Controls.Add(tabControlClientes, 0, 1);
            layoutNavClientes.Dock = DockStyle.Fill;
            layoutNavClientes.Location = new Point(0, 0);
            layoutNavClientes.Margin = new Padding(0);
            layoutNavClientes.Name = "layoutNavClientes";
            layoutNavClientes.RowCount = 2;
            layoutNavClientes.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layoutNavClientes.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutNavClientes.Size = new Size(1080, 720);
            layoutNavClientes.TabIndex = 0;
            layoutNavClientes.Paint += layoutNavClientes_Paint_1;
            // 
            // panelNav
            // 
            panelNav.BackColor = Color.White;
            panelNav.Controls.Add(btnNavClientes);
            panelNav.Controls.Add(btnNavReportes);
            panelNav.Controls.Add(btnNavInventario);
            panelNav.Controls.Add(btnNavHistorial);
            panelNav.Controls.Add(btnNavCaja);
            panelNav.Controls.Add(btnNavEstado);
            panelNav.Controls.Add(btnNavDeudas);
            panelNav.Controls.Add(btnNavPagar);
            panelNav.Controls.Add(btnNavBack);
            panelNav.Dock = DockStyle.Top;
            panelNav.Location = new Point(0, 0);
            panelNav.Margin = new Padding(0);
            panelNav.Name = "panelNav";
            panelNav.Size = new Size(1080, 52);
            panelNav.TabIndex = 0;
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(940, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 8;
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = true;
            // 
            // btnNavReportes
            // 
            btnNavReportes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavReportes.Location = new Point(810, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new Size(120, 32);
            btnNavReportes.TabIndex = 7;
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = true;
            // 
            // btnNavInventario
            // 
            btnNavInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavInventario.Location = new Point(670, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new Size(130, 32);
            btnNavInventario.TabIndex = 6;
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = true;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavHistorial.Location = new Point(540, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new Size(120, 32);
            btnNavHistorial.TabIndex = 5;
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = true;
            // 
            // btnNavCaja
            // 
            btnNavCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavCaja.Location = new Point(420, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new Size(110, 32);
            btnNavCaja.TabIndex = 4;
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = true;
            // 
            // btnNavEstado
            // 
            btnNavEstado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavEstado.Location = new Point(300, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new Size(110, 32);
            btnNavEstado.TabIndex = 3;
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = true;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavDeudas.Location = new Point(180, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new Size(110, 32);
            btnNavDeudas.TabIndex = 2;
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // btnNavPagar
            // 
            btnNavPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnNavPagar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavPagar.ForeColor = Color.White;
            btnNavPagar.Location = new Point(60, 10);
            btnNavPagar.Name = "btnNavPagar";
            btnNavPagar.Size = new Size(110, 32);
            btnNavPagar.TabIndex = 1;
            btnNavPagar.Text = "COBRAR";
            btnNavPagar.UseVisualStyleBackColor = false;
            // 
            // btnNavBack
            // 
            btnNavBack.FlatAppearance.BorderSize = 0;
            btnNavBack.FlatStyle = FlatStyle.Flat;
            btnNavBack.Font = new Font("Segoe UI", 11F);
            btnNavBack.Location = new Point(8, 8);
            btnNavBack.Name = "btnNavBack";
            btnNavBack.Size = new Size(43, 35);
            btnNavBack.TabIndex = 0;
            btnNavBack.UseVisualStyleBackColor = true;
            // 
            // tabControlClientes
            // 
            tabControlClientes.Controls.Add(tabAgregar);
            tabControlClientes.Controls.Add(tabMiembros);
            tabControlClientes.Dock = DockStyle.Fill;
            tabControlClientes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            tabControlClientes.Location = new Point(0, 52);
            tabControlClientes.Margin = new Padding(0);
            tabControlClientes.Name = "tabControlClientes";
            tabControlClientes.SelectedIndex = 0;
            tabControlClientes.Size = new Size(1080, 668);
            tabControlClientes.TabIndex = 1;
            tabControlClientes.SelectedIndexChanged += tabControlClientes_SelectedIndexChanged;
            // 
            // tabAgregar
            // 
            tabAgregar.AutoScroll = true;
            tabAgregar.AutoScrollMinSize = new Size(1200, 1350);
            tabAgregar.BackColor = Color.White;
            tabAgregar.Controls.Add(txtLesionDescripcion);
            tabAgregar.Controls.Add(cmbsexo);
            tabAgregar.Controls.Add(label16);
            tabAgregar.Controls.Add(label15);
            tabAgregar.Controls.Add(chkAsma);
            tabAgregar.Controls.Add(chkHipertension);
            tabAgregar.Controls.Add(chkDiabetes);
            tabAgregar.Controls.Add(label14);
            tabAgregar.Controls.Add(label13);
            tabAgregar.Controls.Add(txtEmergenciaTelefonoAlt);
            tabAgregar.Controls.Add(txtEmergenciaTelefono);
            tabAgregar.Controls.Add(txtEmergenciaParentesco);
            tabAgregar.Controls.Add(txtEmergenciaNombre);
            tabAgregar.Controls.Add(label12);
            tabAgregar.Controls.Add(label11);
            tabAgregar.Controls.Add(label10);
            tabAgregar.Controls.Add(label9);
            tabAgregar.Controls.Add(label8);
            tabAgregar.Controls.Add(lblEdad);
            tabAgregar.Controls.Add(btnAgregar);
            tabAgregar.Controls.Add(dtpFechaIngreso);
            tabAgregar.Controls.Add(label7);
            tabAgregar.Controls.Add(txtDireccion);
            tabAgregar.Controls.Add(label3);
            tabAgregar.Controls.Add(txtTelefono);
            tabAgregar.Controls.Add(label4);
            tabAgregar.Controls.Add(txtFecha);
            tabAgregar.Controls.Add(label5);
            tabAgregar.Controls.Add(txtNombre);
            tabAgregar.Controls.Add(label1);
            tabAgregar.Controls.Add(txtId);
            tabAgregar.Controls.Add(label2);
            tabAgregar.Controls.Add(chkProblemasCardiacos);
            tabAgregar.Controls.Add(chkColesterolAlto);
            tabAgregar.Controls.Add(chkArtritis);
            tabAgregar.Controls.Add(chkHernia);
            tabAgregar.Controls.Add(chkEpilepsia);
            tabAgregar.Controls.Add(chkEmbarazo);
            tabAgregar.Controls.Add(chkNingunaEnfermedad);
            tabAgregar.Controls.Add(lblEnfermedadOtra);
            tabAgregar.Controls.Add(txtEnfermedadOtra);
            tabAgregar.Controls.Add(lblLesiones);
            tabAgregar.Controls.Add(chkLesionHombro);
            tabAgregar.Controls.Add(chkLesionRodilla);
            tabAgregar.Controls.Add(chkLesionEspalda);
            tabAgregar.Controls.Add(chkLesionCuello);
            tabAgregar.Controls.Add(chkLesionTobillo);
            tabAgregar.Controls.Add(chkLesionCadera);
            tabAgregar.Controls.Add(lblLesionDescripcion);
            tabAgregar.Controls.Add(lblMedicamentos);
            tabAgregar.Controls.Add(panelMedicamentos);
            tabAgregar.Controls.Add(lblListaMedicamentos);
            tabAgregar.Controls.Add(txtListaMedicamentos);
            tabAgregar.Controls.Add(lblAlergias);
            tabAgregar.Controls.Add(panelAlergias);
            tabAgregar.Controls.Add(lblAlergiasDescripcion);
            tabAgregar.Controls.Add(txtAlergiasDescripcion);
            tabAgregar.Controls.Add(lblCirugias);
            tabAgregar.Controls.Add(panelCirugias);
            tabAgregar.Controls.Add(lblCirugiasDescripcion);
            tabAgregar.Controls.Add(txtCirugiasDescripcion);
            tabAgregar.Controls.Add(lblCirugiasFecha);
            tabAgregar.Controls.Add(dtpCirugiasFecha);
            tabAgregar.Controls.Add(lblCirugiaAntiguedad);
            tabAgregar.Controls.Add(lblObjetivoFitness);
            tabAgregar.Controls.Add(lblObjetivoHint);
            tabAgregar.Controls.Add(chkObjPerderGrasa);
            tabAgregar.Controls.Add(chkObjGanarMasa);
            tabAgregar.Controls.Add(chkObjTonificar);
            tabAgregar.Controls.Add(chkObjMejorarCondicion);
            tabAgregar.Controls.Add(chkObjRehabilitacion);
            tabAgregar.Controls.Add(chkObjSalud);
            tabAgregar.Controls.Add(chkObjCompetencia);
            tabAgregar.Controls.Add(chkObjOtro);
            tabAgregar.Controls.Add(txtObjOtroDescripcion);
            tabAgregar.Controls.Add(lblExperiencia);
            tabAgregar.Controls.Add(panelExperiencia);
            tabAgregar.Controls.Add(lblHorarioPreferido);
            tabAgregar.Controls.Add(panelHorario);
            tabAgregar.Location = new Point(4, 34);
            tabAgregar.Name = "tabAgregar";
            tabAgregar.Padding = new Padding(24);
            tabAgregar.Size = new Size(1072, 630);
            tabAgregar.TabIndex = 0;
            tabAgregar.Text = "AGREGAR CLIENTES";
            // 
            // txtLesionDescripcion
            // 
            txtLesionDescripcion.Font = new Font("Segoe UI", 11F);
            txtLesionDescripcion.Location = new Point(771, 340);
            txtLesionDescripcion.Name = "txtLesionDescripcion";
            txtLesionDescripcion.Size = new Size(389, 32);
            txtLesionDescripcion.TabIndex = 47;
            // 
            // cmbsexo
            // 
            cmbsexo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbsexo.FormattingEnabled = true;
            cmbsexo.Items.AddRange(new object[] { "Masculino", "Femenino", "Otro" });
            cmbsexo.Location = new Point(71, 346);
            cmbsexo.Name = "cmbsexo";
            cmbsexo.Size = new Size(151, 33);
            cmbsexo.TabIndex = 78;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label16.Location = new Point(16, 350);
            label16.Name = "label16";
            label16.Size = new Size(53, 23);
            label16.TabIndex = 77;
            label16.Text = "SEXO";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label15.Location = new Point(434, 121);
            label15.Name = "label15";
            label15.Size = new Size(57, 23);
            label15.TabIndex = 29;
            label15.Text = "EDAD";
            // 
            // chkAsma
            // 
            chkAsma.AutoSize = true;
            chkAsma.Font = new Font("Segoe UI", 10F);
            chkAsma.Location = new Point(1040, 90);
            chkAsma.Name = "chkAsma";
            chkAsma.Size = new Size(74, 27);
            chkAsma.TabIndex = 28;
            chkAsma.Text = "Asma";
            chkAsma.UseVisualStyleBackColor = true;
            // 
            // chkHipertension
            // 
            chkHipertension.AutoSize = true;
            chkHipertension.Font = new Font("Segoe UI", 10F);
            chkHipertension.Location = new Point(850, 90);
            chkHipertension.Name = "chkHipertension";
            chkHipertension.Size = new Size(129, 27);
            chkHipertension.TabIndex = 27;
            chkHipertension.Text = "Hipertensión";
            chkHipertension.UseVisualStyleBackColor = true;
            // 
            // chkDiabetes
            // 
            chkDiabetes.AutoSize = true;
            chkDiabetes.Font = new Font("Segoe UI", 10F);
            chkDiabetes.Location = new Point(662, 90);
            chkDiabetes.Name = "chkDiabetes";
            chkDiabetes.Size = new Size(98, 27);
            chkDiabetes.TabIndex = 26;
            chkDiabetes.Text = "Diabetes";
            chkDiabetes.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            label14.ForeColor = Color.FromArgb(15, 23, 42);
            label14.Location = new Point(749, 53);
            label14.Name = "label14";
            label14.Size = new Size(183, 30);
            label14.TabIndex = 25;
            label14.Text = "ENFERMEDADES";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label13.Location = new Point(693, 18);
            label13.Name = "label13";
            label13.Size = new Size(300, 35);
            label13.TabIndex = 24;
            label13.Text = "INFORMACIÓN MÉDICA";
            // 
            // txtEmergenciaTelefonoAlt
            // 
            txtEmergenciaTelefonoAlt.Font = new Font("Segoe UI", 11F);
            txtEmergenciaTelefonoAlt.Location = new Point(192, 595);
            txtEmergenciaTelefonoAlt.Name = "txtEmergenciaTelefonoAlt";
            txtEmergenciaTelefonoAlt.Size = new Size(322, 32);
            txtEmergenciaTelefonoAlt.TabIndex = 23;
            // 
            // txtEmergenciaTelefono
            // 
            txtEmergenciaTelefono.Font = new Font("Segoe UI", 11F);
            txtEmergenciaTelefono.Location = new Point(126, 550);
            txtEmergenciaTelefono.Name = "txtEmergenciaTelefono";
            txtEmergenciaTelefono.Size = new Size(322, 32);
            txtEmergenciaTelefono.TabIndex = 22;
            // 
            // txtEmergenciaParentesco
            // 
            txtEmergenciaParentesco.Font = new Font("Segoe UI", 11F);
            txtEmergenciaParentesco.Location = new Point(145, 506);
            txtEmergenciaParentesco.Name = "txtEmergenciaParentesco";
            txtEmergenciaParentesco.Size = new Size(322, 32);
            txtEmergenciaParentesco.TabIndex = 21;
            // 
            // txtEmergenciaNombre
            // 
            txtEmergenciaNombre.Font = new Font("Segoe UI", 11F);
            txtEmergenciaNombre.Location = new Point(112, 468);
            txtEmergenciaNombre.Name = "txtEmergenciaNombre";
            txtEmergenciaNombre.Size = new Size(322, 32);
            txtEmergenciaNombre.TabIndex = 15;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label12.Location = new Point(27, 599);
            label12.Name = "label12";
            label12.Size = new Size(161, 23);
            label12.TabIndex = 20;
            label12.Text = "TEL. ALTERNATIVO";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label11.Location = new Point(27, 511);
            label11.Name = "label11";
            label11.Size = new Size(117, 23);
            label11.TabIndex = 19;
            label11.Text = "PARENTESCO";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label10.Location = new Point(27, 554);
            label10.Name = "label10";
            label10.Size = new Size(95, 23);
            label10.TabIndex = 18;
            label10.Text = "TELEFONO";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label9.Location = new Point(27, 471);
            label9.Name = "label9";
            label9.Size = new Size(83, 23);
            label9.TabIndex = 17;
            label9.Text = "NOMBRE";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label8.Location = new Point(16, 404);
            label8.Name = "label8";
            label8.Size = new Size(354, 35);
            label8.TabIndex = 16;
            label8.Text = "CONTACTO DE EMERGENCIA";
            // 
            // lblEdad
            // 
            lblEdad.AutoSize = true;
            lblEdad.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblEdad.Location = new Point(489, 113);
            lblEdad.Name = "lblEdad";
            lblEdad.Size = new Size(29, 35);
            lblEdad.TabIndex = 15;
            lblEdad.Text = "0";
            // 
            // btnAgregar
            // 
            btnAgregar.BackColor = Color.FromArgb(0, 122, 204);
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAgregar.ForeColor = Color.White;
            btnAgregar.Location = new Point(812, 1265);
            btnAgregar.Name = "btnAgregar";
            btnAgregar.Size = new Size(177, 55);
            btnAgregar.TabIndex = 12;
            btnAgregar.Text = "AGREGAR";
            btnAgregar.UseVisualStyleBackColor = false;
            btnAgregar.Click += btnAgregar_Click;
            // 
            // dtpFechaIngreso
            // 
            dtpFechaIngreso.Font = new Font("Segoe UI", 11F);
            dtpFechaIngreso.Location = new Point(190, 284);
            dtpFechaIngreso.Name = "dtpFechaIngreso";
            dtpFechaIngreso.Size = new Size(233, 32);
            dtpFechaIngreso.TabIndex = 11;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label7.Location = new Point(16, 288);
            label7.Name = "label7";
            label7.Size = new Size(169, 23);
            label7.TabIndex = 10;
            label7.Text = "FECHA DE INGRESO";
            // 
            // txtDireccion
            // 
            txtDireccion.Font = new Font("Segoe UI", 11F);
            txtDireccion.Location = new Point(120, 229);
            txtDireccion.Name = "txtDireccion";
            txtDireccion.Size = new Size(322, 32);
            txtDireccion.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.Location = new Point(16, 233);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 8;
            label3.Text = "DIRECCION";
            // 
            // txtTelefono
            // 
            txtTelefono.Font = new Font("Segoe UI", 11F);
            txtTelefono.Location = new Point(112, 174);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(203, 32);
            txtTelefono.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label4.Location = new Point(16, 178);
            label4.Name = "label4";
            label4.Size = new Size(95, 23);
            label4.TabIndex = 6;
            label4.Text = "TELEFONO";
            // 
            // txtFecha
            // 
            txtFecha.CustomFormat = "dd-MMMM-yyyy";
            txtFecha.Font = new Font("Segoe UI", 11F);
            txtFecha.Format = DateTimePickerFormat.Custom;
            txtFecha.Location = new Point(226, 118);
            txtFecha.Name = "txtFecha";
            txtFecha.Size = new Size(203, 32);
            txtFecha.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label5.Location = new Point(16, 123);
            label5.Name = "label5";
            label5.Size = new Size(202, 23);
            label5.TabIndex = 4;
            label5.Text = "FECHA DE NACIMIENTO";
            // 
            // txtNombre
            // 
            txtNombre.Font = new Font("Segoe UI", 11F);
            txtNombre.Location = new Point(206, 64);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(322, 32);
            txtNombre.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(16, 68);
            label1.Name = "label1";
            label1.Size = new Size(183, 23);
            label1.TabIndex = 2;
            label1.Text = "NOMBRE Y APELLIDO";
            // 
            // txtId
            // 
            txtId.Font = new Font("Segoe UI", 11F);
            txtId.Location = new Point(96, 14);
            txtId.Name = "txtId";
            txtId.ReadOnly = true;
            txtId.Size = new Size(123, 32);
            txtId.TabIndex = 1;
            txtId.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.Location = new Point(16, 18);
            label2.Name = "label2";
            label2.Size = new Size(28, 23);
            label2.TabIndex = 0;
            label2.Text = "ID";
            label2.Visible = false;
            // 
            // chkProblemasCardiacos
            // 
            chkProblemasCardiacos.AutoSize = true;
            chkProblemasCardiacos.Font = new Font("Segoe UI", 10F);
            chkProblemasCardiacos.Location = new Point(662, 122);
            chkProblemasCardiacos.Name = "chkProblemasCardiacos";
            chkProblemasCardiacos.Size = new Size(188, 27);
            chkProblemasCardiacos.TabIndex = 30;
            chkProblemasCardiacos.Text = "Problemas cardíacos";
            chkProblemasCardiacos.UseVisualStyleBackColor = true;
            // 
            // chkColesterolAlto
            // 
            chkColesterolAlto.AutoSize = true;
            chkColesterolAlto.Font = new Font("Segoe UI", 10F);
            chkColesterolAlto.Location = new Point(850, 122);
            chkColesterolAlto.Name = "chkColesterolAlto";
            chkColesterolAlto.Size = new Size(142, 27);
            chkColesterolAlto.TabIndex = 31;
            chkColesterolAlto.Text = "Colesterol alto";
            chkColesterolAlto.UseVisualStyleBackColor = true;
            // 
            // chkArtritis
            // 
            chkArtritis.AutoSize = true;
            chkArtritis.Font = new Font("Segoe UI", 10F);
            chkArtritis.Location = new Point(1040, 122);
            chkArtritis.Name = "chkArtritis";
            chkArtritis.Size = new Size(82, 27);
            chkArtritis.TabIndex = 32;
            chkArtritis.Text = "Artritis";
            chkArtritis.UseVisualStyleBackColor = true;
            // 
            // chkHernia
            // 
            chkHernia.AutoSize = true;
            chkHernia.Font = new Font("Segoe UI", 10F);
            chkHernia.Location = new Point(662, 154);
            chkHernia.Name = "chkHernia";
            chkHernia.Size = new Size(82, 27);
            chkHernia.TabIndex = 33;
            chkHernia.Text = "Hernia";
            chkHernia.UseVisualStyleBackColor = true;
            // 
            // chkEpilepsia
            // 
            chkEpilepsia.AutoSize = true;
            chkEpilepsia.Font = new Font("Segoe UI", 10F);
            chkEpilepsia.Location = new Point(850, 154);
            chkEpilepsia.Name = "chkEpilepsia";
            chkEpilepsia.Size = new Size(98, 27);
            chkEpilepsia.TabIndex = 34;
            chkEpilepsia.Text = "Epilepsia";
            chkEpilepsia.UseVisualStyleBackColor = true;
            // 
            // chkEmbarazo
            // 
            chkEmbarazo.AutoSize = true;
            chkEmbarazo.Font = new Font("Segoe UI", 10F);
            chkEmbarazo.Location = new Point(1040, 154);
            chkEmbarazo.Name = "chkEmbarazo";
            chkEmbarazo.Size = new Size(108, 27);
            chkEmbarazo.TabIndex = 35;
            chkEmbarazo.Text = "Embarazo";
            chkEmbarazo.UseVisualStyleBackColor = true;
            // 
            // chkNingunaEnfermedad
            // 
            chkNingunaEnfermedad.AutoSize = true;
            chkNingunaEnfermedad.Font = new Font("Segoe UI", 10F);
            chkNingunaEnfermedad.Location = new Point(662, 186);
            chkNingunaEnfermedad.Name = "chkNingunaEnfermedad";
            chkNingunaEnfermedad.Size = new Size(98, 27);
            chkNingunaEnfermedad.TabIndex = 36;
            chkNingunaEnfermedad.Text = "Ninguna";
            chkNingunaEnfermedad.UseVisualStyleBackColor = true;
            // 
            // lblEnfermedadOtra
            // 
            lblEnfermedadOtra.AutoSize = true;
            lblEnfermedadOtra.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblEnfermedadOtra.Location = new Point(850, 188);
            lblEnfermedadOtra.Name = "lblEnfermedadOtra";
            lblEnfermedadOtra.Size = new Size(52, 23);
            lblEnfermedadOtra.TabIndex = 37;
            lblEnfermedadOtra.Text = "Otro:";
            // 
            // txtEnfermedadOtra
            // 
            txtEnfermedadOtra.Font = new Font("Segoe UI", 11F);
            txtEnfermedadOtra.Location = new Point(905, 186);
            txtEnfermedadOtra.Name = "txtEnfermedadOtra";
            txtEnfermedadOtra.Size = new Size(280, 32);
            txtEnfermedadOtra.TabIndex = 38;
            // 
            // lblLesiones
            // 
            lblLesiones.AutoSize = true;
            lblLesiones.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblLesiones.ForeColor = Color.FromArgb(15, 23, 42);
            lblLesiones.Location = new Point(749, 235);
            lblLesiones.Name = "lblLesiones";
            lblLesiones.Size = new Size(113, 30);
            lblLesiones.TabIndex = 39;
            lblLesiones.Text = "LESIONES";
            // 
            // chkLesionHombro
            // 
            chkLesionHombro.AutoSize = true;
            chkLesionHombro.Font = new Font("Segoe UI", 10F);
            chkLesionHombro.Location = new Point(662, 275);
            chkLesionHombro.Name = "chkLesionHombro";
            chkLesionHombro.Size = new Size(95, 27);
            chkLesionHombro.TabIndex = 40;
            chkLesionHombro.Text = "Hombro";
            chkLesionHombro.UseVisualStyleBackColor = true;
            // 
            // chkLesionRodilla
            // 
            chkLesionRodilla.AutoSize = true;
            chkLesionRodilla.Font = new Font("Segoe UI", 10F);
            chkLesionRodilla.Location = new Point(850, 275);
            chkLesionRodilla.Name = "chkLesionRodilla";
            chkLesionRodilla.Size = new Size(83, 27);
            chkLesionRodilla.TabIndex = 41;
            chkLesionRodilla.Text = "Rodilla";
            chkLesionRodilla.UseVisualStyleBackColor = true;
            // 
            // chkLesionEspalda
            // 
            chkLesionEspalda.AutoSize = true;
            chkLesionEspalda.Font = new Font("Segoe UI", 10F);
            chkLesionEspalda.Location = new Point(1040, 275);
            chkLesionEspalda.Name = "chkLesionEspalda";
            chkLesionEspalda.Size = new Size(90, 27);
            chkLesionEspalda.TabIndex = 42;
            chkLesionEspalda.Text = "Espalda";
            chkLesionEspalda.UseVisualStyleBackColor = true;
            // 
            // chkLesionCuello
            // 
            chkLesionCuello.AutoSize = true;
            chkLesionCuello.Font = new Font("Segoe UI", 10F);
            chkLesionCuello.Location = new Point(662, 307);
            chkLesionCuello.Name = "chkLesionCuello";
            chkLesionCuello.Size = new Size(80, 27);
            chkLesionCuello.TabIndex = 43;
            chkLesionCuello.Text = "Cuello";
            chkLesionCuello.UseVisualStyleBackColor = true;
            // 
            // chkLesionTobillo
            // 
            chkLesionTobillo.AutoSize = true;
            chkLesionTobillo.Font = new Font("Segoe UI", 10F);
            chkLesionTobillo.Location = new Point(850, 307);
            chkLesionTobillo.Name = "chkLesionTobillo";
            chkLesionTobillo.Size = new Size(81, 27);
            chkLesionTobillo.TabIndex = 44;
            chkLesionTobillo.Text = "Tobillo";
            chkLesionTobillo.UseVisualStyleBackColor = true;
            // 
            // chkLesionCadera
            // 
            chkLesionCadera.AutoSize = true;
            chkLesionCadera.Font = new Font("Segoe UI", 10F);
            chkLesionCadera.Location = new Point(1040, 307);
            chkLesionCadera.Name = "chkLesionCadera";
            chkLesionCadera.Size = new Size(86, 27);
            chkLesionCadera.TabIndex = 45;
            chkLesionCadera.Text = "Cadera";
            chkLesionCadera.UseVisualStyleBackColor = true;
            // 
            // lblLesionDescripcion
            // 
            lblLesionDescripcion.AutoSize = true;
            lblLesionDescripcion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLesionDescripcion.Location = new Point(662, 345);
            lblLesionDescripcion.Name = "lblLesionDescripcion";
            lblLesionDescripcion.Size = new Size(103, 23);
            lblLesionDescripcion.TabIndex = 46;
            lblLesionDescripcion.Text = "Descripción";
            // 
            // lblMedicamentos
            // 
            lblMedicamentos.AutoSize = true;
            lblMedicamentos.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblMedicamentos.ForeColor = Color.FromArgb(15, 23, 42);
            lblMedicamentos.Location = new Point(749, 386);
            lblMedicamentos.Name = "lblMedicamentos";
            lblMedicamentos.Size = new Size(189, 30);
            lblMedicamentos.TabIndex = 48;
            lblMedicamentos.Text = "MEDICAMENTOS";
            // 
            // panelMedicamentos
            // 
            panelMedicamentos.BackColor = Color.Transparent;
            panelMedicamentos.Controls.Add(rbMedicamentosSi);
            panelMedicamentos.Controls.Add(rbMedicamentosNo);
            panelMedicamentos.Location = new Point(680, 426);
            panelMedicamentos.Name = "panelMedicamentos";
            panelMedicamentos.Size = new Size(170, 36);
            panelMedicamentos.TabIndex = 49;
            // 
            // rbMedicamentosSi
            // 
            rbMedicamentosSi.AutoSize = true;
            rbMedicamentosSi.Font = new Font("Segoe UI", 10F);
            rbMedicamentosSi.Location = new Point(26, 4);
            rbMedicamentosSi.Name = "rbMedicamentosSi";
            rbMedicamentosSi.Size = new Size(44, 27);
            rbMedicamentosSi.TabIndex = 0;
            rbMedicamentosSi.Text = "Sí";
            rbMedicamentosSi.UseVisualStyleBackColor = true;
            // 
            // rbMedicamentosNo
            // 
            rbMedicamentosNo.AutoSize = true;
            rbMedicamentosNo.Checked = true;
            rbMedicamentosNo.Font = new Font("Segoe UI", 10F);
            rbMedicamentosNo.Location = new Point(96, 4);
            rbMedicamentosNo.Name = "rbMedicamentosNo";
            rbMedicamentosNo.Size = new Size(54, 27);
            rbMedicamentosNo.TabIndex = 1;
            rbMedicamentosNo.TabStop = true;
            rbMedicamentosNo.Text = "No";
            rbMedicamentosNo.UseVisualStyleBackColor = true;
            // 
            // lblListaMedicamentos
            // 
            lblListaMedicamentos.AutoSize = true;
            lblListaMedicamentos.Enabled = false;
            lblListaMedicamentos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblListaMedicamentos.Location = new Point(662, 468);
            lblListaMedicamentos.Name = "lblListaMedicamentos";
            lblListaMedicamentos.Size = new Size(194, 23);
            lblListaMedicamentos.TabIndex = 50;
            lblListaMedicamentos.Text = "Lista de medicamentos";
            // 
            // txtListaMedicamentos
            // 
            txtListaMedicamentos.Enabled = false;
            txtListaMedicamentos.Font = new Font("Segoe UI", 11F);
            txtListaMedicamentos.Location = new Point(662, 495);
            txtListaMedicamentos.Name = "txtListaMedicamentos";
            txtListaMedicamentos.PlaceholderText = "Ej: Losartán 50mg, Metformina...";
            txtListaMedicamentos.Size = new Size(560, 32);
            txtListaMedicamentos.TabIndex = 51;
            // 
            // lblAlergias
            // 
            lblAlergias.AutoSize = true;
            lblAlergias.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblAlergias.ForeColor = Color.FromArgb(15, 23, 42);
            lblAlergias.Location = new Point(749, 545);
            lblAlergias.Name = "lblAlergias";
            lblAlergias.Size = new Size(115, 30);
            lblAlergias.TabIndex = 52;
            lblAlergias.Text = "ALERGIAS";
            // 
            // panelAlergias
            // 
            panelAlergias.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelAlergias.BackColor = Color.Transparent;
            panelAlergias.Controls.Add(rbAlergiasSi);
            panelAlergias.Controls.Add(rbAlergiasNo);
            panelAlergias.Location = new Point(662, 581);
            panelAlergias.Name = "panelAlergias";
            panelAlergias.Size = new Size(0, 36);
            panelAlergias.TabIndex = 53;
            // 
            // rbAlergiasSi
            // 
            rbAlergiasSi.AutoSize = true;
            rbAlergiasSi.Font = new Font("Segoe UI", 10F);
            rbAlergiasSi.Location = new Point(18, 4);
            rbAlergiasSi.Name = "rbAlergiasSi";
            rbAlergiasSi.Size = new Size(44, 27);
            rbAlergiasSi.TabIndex = 0;
            rbAlergiasSi.Text = "Sí";
            rbAlergiasSi.UseVisualStyleBackColor = true;
            // 
            // rbAlergiasNo
            // 
            rbAlergiasNo.AutoSize = true;
            rbAlergiasNo.Checked = true;
            rbAlergiasNo.Font = new Font("Segoe UI", 10F);
            rbAlergiasNo.Location = new Point(94, 3);
            rbAlergiasNo.Name = "rbAlergiasNo";
            rbAlergiasNo.Size = new Size(54, 27);
            rbAlergiasNo.TabIndex = 1;
            rbAlergiasNo.TabStop = true;
            rbAlergiasNo.Text = "No";
            rbAlergiasNo.UseVisualStyleBackColor = true;
            // 
            // lblAlergiasDescripcion
            // 
            lblAlergiasDescripcion.AutoSize = true;
            lblAlergiasDescripcion.Enabled = false;
            lblAlergiasDescripcion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAlergiasDescripcion.Location = new Point(662, 623);
            lblAlergiasDescripcion.Name = "lblAlergiasDescripcion";
            lblAlergiasDescripcion.Size = new Size(103, 23);
            lblAlergiasDescripcion.TabIndex = 54;
            lblAlergiasDescripcion.Text = "Descripción";
            // 
            // txtAlergiasDescripcion
            // 
            txtAlergiasDescripcion.Enabled = false;
            txtAlergiasDescripcion.Font = new Font("Segoe UI", 11F);
            txtAlergiasDescripcion.Location = new Point(662, 650);
            txtAlergiasDescripcion.Name = "txtAlergiasDescripcion";
            txtAlergiasDescripcion.PlaceholderText = "Describa las alergias...";
            txtAlergiasDescripcion.Size = new Size(560, 32);
            txtAlergiasDescripcion.TabIndex = 55;
            // 
            // lblCirugias
            // 
            lblCirugias.AutoSize = true;
            lblCirugias.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblCirugias.ForeColor = Color.FromArgb(15, 23, 42);
            lblCirugias.Location = new Point(749, 700);
            lblCirugias.Name = "lblCirugias";
            lblCirugias.Size = new Size(114, 30);
            lblCirugias.TabIndex = 56;
            lblCirugias.Text = "CIRUGÍAS";
            // 
            // panelCirugias
            // 
            panelCirugias.BackColor = Color.Transparent;
            panelCirugias.Controls.Add(rbCirugiasSi);
            panelCirugias.Controls.Add(rbCirugiasNo);
            panelCirugias.Location = new Point(662, 736);
            panelCirugias.Name = "panelCirugias";
            panelCirugias.Size = new Size(200, 36);
            panelCirugias.TabIndex = 57;
            // 
            // rbCirugiasSi
            // 
            rbCirugiasSi.AutoSize = true;
            rbCirugiasSi.Font = new Font("Segoe UI", 10F);
            rbCirugiasSi.Location = new Point(25, 4);
            rbCirugiasSi.Name = "rbCirugiasSi";
            rbCirugiasSi.Size = new Size(44, 27);
            rbCirugiasSi.TabIndex = 0;
            rbCirugiasSi.Text = "Sí";
            rbCirugiasSi.UseVisualStyleBackColor = true;
            // 
            // rbCirugiasNo
            // 
            rbCirugiasNo.AutoSize = true;
            rbCirugiasNo.Checked = true;
            rbCirugiasNo.Font = new Font("Segoe UI", 10F);
            rbCirugiasNo.Location = new Point(95, 4);
            rbCirugiasNo.Name = "rbCirugiasNo";
            rbCirugiasNo.Size = new Size(54, 27);
            rbCirugiasNo.TabIndex = 1;
            rbCirugiasNo.TabStop = true;
            rbCirugiasNo.Text = "No";
            rbCirugiasNo.UseVisualStyleBackColor = true;
            // 
            // lblCirugiasDescripcion
            // 
            lblCirugiasDescripcion.AutoSize = true;
            lblCirugiasDescripcion.Enabled = false;
            lblCirugiasDescripcion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCirugiasDescripcion.Location = new Point(662, 786);
            lblCirugiasDescripcion.Name = "lblCirugiasDescripcion";
            lblCirugiasDescripcion.Size = new Size(103, 23);
            lblCirugiasDescripcion.TabIndex = 58;
            lblCirugiasDescripcion.Text = "Descripción";
            // 
            // txtCirugiasDescripcion
            // 
            txtCirugiasDescripcion.Enabled = false;
            txtCirugiasDescripcion.Font = new Font("Segoe UI", 11F);
            txtCirugiasDescripcion.Location = new Point(772, 783);
            txtCirugiasDescripcion.Name = "txtCirugiasDescripcion";
            txtCirugiasDescripcion.PlaceholderText = "Describa la cirugía...";
            txtCirugiasDescripcion.Size = new Size(450, 32);
            txtCirugiasDescripcion.TabIndex = 59;
            // 
            // lblCirugiasFecha
            // 
            lblCirugiasFecha.AutoSize = true;
            lblCirugiasFecha.Enabled = false;
            lblCirugiasFecha.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCirugiasFecha.Location = new Point(662, 832);
            lblCirugiasFecha.Name = "lblCirugiasFecha";
            lblCirugiasFecha.Size = new Size(55, 23);
            lblCirugiasFecha.TabIndex = 60;
            lblCirugiasFecha.Text = "Fecha";
            // 
            // dtpCirugiasFecha
            // 
            dtpCirugiasFecha.CustomFormat = "dd-MMMM-yyyy";
            dtpCirugiasFecha.Enabled = false;
            dtpCirugiasFecha.Font = new Font("Segoe UI", 11F);
            dtpCirugiasFecha.Format = DateTimePickerFormat.Custom;
            dtpCirugiasFecha.Location = new Point(721, 828);
            dtpCirugiasFecha.MinDate = new DateTime(1950, 1, 1, 0, 0, 0, 0);
            dtpCirugiasFecha.Name = "dtpCirugiasFecha";
            dtpCirugiasFecha.Size = new Size(220, 32);
            dtpCirugiasFecha.TabIndex = 61;
            // 
            // lblCirugiaAntiguedad
            // 
            lblCirugiaAntiguedad.AutoSize = true;
            lblCirugiaAntiguedad.Enabled = false;
            lblCirugiaAntiguedad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCirugiaAntiguedad.ForeColor = Color.FromArgb(0, 122, 204);
            lblCirugiaAntiguedad.Location = new Point(1005, 818);
            lblCirugiaAntiguedad.Name = "lblCirugiaAntiguedad";
            lblCirugiaAntiguedad.Size = new Size(0, 23);
            lblCirugiaAntiguedad.TabIndex = 61;
            // 
            // lblObjetivoFitness
            // 
            lblObjetivoFitness.AutoSize = true;
            lblObjetivoFitness.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblObjetivoFitness.ForeColor = Color.FromArgb(15, 23, 42);
            lblObjetivoFitness.Location = new Point(720, 870);
            lblObjetivoFitness.Name = "lblObjetivoFitness";
            lblObjetivoFitness.Size = new Size(208, 30);
            lblObjetivoFitness.TabIndex = 62;
            lblObjetivoFitness.Text = "OBJETIVO FITNESS";
            // 
            // lblObjetivoHint
            // 
            lblObjetivoHint.AutoSize = true;
            lblObjetivoHint.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblObjetivoHint.ForeColor = Color.FromArgb(100, 116, 139);
            lblObjetivoHint.Location = new Point(933, 876);
            lblObjetivoHint.Name = "lblObjetivoHint";
            lblObjetivoHint.Size = new Size(138, 20);
            lblObjetivoHint.TabIndex = 63;
            lblObjetivoHint.Text = "(puede elegir varios)";
            // 
            // chkObjPerderGrasa
            // 
            chkObjPerderGrasa.AutoSize = true;
            chkObjPerderGrasa.Font = new Font("Segoe UI", 10F);
            chkObjPerderGrasa.Location = new Point(662, 910);
            chkObjPerderGrasa.Name = "chkObjPerderGrasa";
            chkObjPerderGrasa.Size = new Size(127, 27);
            chkObjPerderGrasa.TabIndex = 64;
            chkObjPerderGrasa.Text = "Perder grasa";
            chkObjPerderGrasa.UseVisualStyleBackColor = true;
            // 
            // chkObjGanarMasa
            // 
            chkObjGanarMasa.AutoSize = true;
            chkObjGanarMasa.Font = new Font("Segoe UI", 10F);
            chkObjGanarMasa.Location = new Point(850, 910);
            chkObjGanarMasa.Name = "chkObjGanarMasa";
            chkObjGanarMasa.Size = new Size(197, 27);
            chkObjGanarMasa.TabIndex = 65;
            chkObjGanarMasa.Text = "Ganar masa muscular";
            chkObjGanarMasa.UseVisualStyleBackColor = true;
            // 
            // chkObjTonificar
            // 
            chkObjTonificar.AutoSize = true;
            chkObjTonificar.Font = new Font("Segoe UI", 10F);
            chkObjTonificar.Location = new Point(1051, 910);
            chkObjTonificar.Name = "chkObjTonificar";
            chkObjTonificar.Size = new Size(95, 27);
            chkObjTonificar.TabIndex = 66;
            chkObjTonificar.Text = "Tonificar";
            chkObjTonificar.UseVisualStyleBackColor = true;
            // 
            // chkObjMejorarCondicion
            // 
            chkObjMejorarCondicion.AutoSize = true;
            chkObjMejorarCondicion.Font = new Font("Segoe UI", 10F);
            chkObjMejorarCondicion.Location = new Point(662, 942);
            chkObjMejorarCondicion.Name = "chkObjMejorarCondicion";
            chkObjMejorarCondicion.Size = new Size(212, 27);
            chkObjMejorarCondicion.TabIndex = 67;
            chkObjMejorarCondicion.Text = "Mejorar condición física";
            chkObjMejorarCondicion.UseVisualStyleBackColor = true;
            // 
            // chkObjRehabilitacion
            // 
            chkObjRehabilitacion.AutoSize = true;
            chkObjRehabilitacion.Font = new Font("Segoe UI", 10F);
            chkObjRehabilitacion.Location = new Point(850, 942);
            chkObjRehabilitacion.Name = "chkObjRehabilitacion";
            chkObjRehabilitacion.Size = new Size(139, 27);
            chkObjRehabilitacion.TabIndex = 68;
            chkObjRehabilitacion.Text = "Rehabilitación";
            chkObjRehabilitacion.UseVisualStyleBackColor = true;
            // 
            // chkObjSalud
            // 
            chkObjSalud.AutoSize = true;
            chkObjSalud.Font = new Font("Segoe UI", 10F);
            chkObjSalud.Location = new Point(1040, 942);
            chkObjSalud.Name = "chkObjSalud";
            chkObjSalud.Size = new Size(74, 27);
            chkObjSalud.TabIndex = 69;
            chkObjSalud.Text = "Salud";
            chkObjSalud.UseVisualStyleBackColor = true;
            // 
            // chkObjCompetencia
            // 
            chkObjCompetencia.AutoSize = true;
            chkObjCompetencia.Font = new Font("Segoe UI", 10F);
            chkObjCompetencia.Location = new Point(662, 974);
            chkObjCompetencia.Name = "chkObjCompetencia";
            chkObjCompetencia.Size = new Size(133, 27);
            chkObjCompetencia.TabIndex = 70;
            chkObjCompetencia.Text = "Competencia";
            chkObjCompetencia.UseVisualStyleBackColor = true;
            // 
            // chkObjOtro
            // 
            chkObjOtro.AutoSize = true;
            chkObjOtro.Font = new Font("Segoe UI", 10F);
            chkObjOtro.Location = new Point(850, 974);
            chkObjOtro.Name = "chkObjOtro";
            chkObjOtro.Size = new Size(67, 27);
            chkObjOtro.TabIndex = 71;
            chkObjOtro.Text = "Otro";
            chkObjOtro.UseVisualStyleBackColor = true;
            // 
            // txtObjOtroDescripcion
            // 
            txtObjOtroDescripcion.Enabled = false;
            txtObjOtroDescripcion.Font = new Font("Segoe UI", 11F);
            txtObjOtroDescripcion.Location = new Point(920, 970);
            txtObjOtroDescripcion.Name = "txtObjOtroDescripcion";
            txtObjOtroDescripcion.PlaceholderText = "Especifique otro objetivo...";
            txtObjOtroDescripcion.Size = new Size(280, 32);
            txtObjOtroDescripcion.TabIndex = 72;
            // 
            // lblExperiencia
            // 
            lblExperiencia.AutoSize = true;
            lblExperiencia.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblExperiencia.ForeColor = Color.FromArgb(15, 23, 42);
            lblExperiencia.Location = new Point(749, 1025);
            lblExperiencia.Name = "lblExperiencia";
            lblExperiencia.Size = new Size(151, 30);
            lblExperiencia.TabIndex = 73;
            lblExperiencia.Text = "EXPERIENCIA";
            // 
            // panelExperiencia
            // 
            panelExperiencia.BackColor = Color.Transparent;
            panelExperiencia.Controls.Add(rbExpNunca);
            panelExperiencia.Controls.Add(rbExpMenos6);
            panelExperiencia.Controls.Add(rbExp1Ano);
            panelExperiencia.Controls.Add(rbExp2Anos);
            panelExperiencia.Controls.Add(rbExpMas5);
            panelExperiencia.Location = new Point(662, 1061);
            panelExperiencia.Name = "panelExperiencia";
            panelExperiencia.Size = new Size(560, 70);
            panelExperiencia.TabIndex = 74;
            // 
            // rbExpNunca
            // 
            rbExpNunca.AutoSize = true;
            rbExpNunca.Font = new Font("Segoe UI", 10F);
            rbExpNunca.Location = new Point(0, 4);
            rbExpNunca.Name = "rbExpNunca";
            rbExpNunca.Size = new Size(146, 27);
            rbExpNunca.TabIndex = 0;
            rbExpNunca.Tag = "Nunca";
            rbExpNunca.Text = "Nunca entrenó";
            rbExpNunca.UseVisualStyleBackColor = true;
            // 
            // rbExpMenos6
            // 
            rbExpMenos6.AutoSize = true;
            rbExpMenos6.Font = new Font("Segoe UI", 10F);
            rbExpMenos6.Location = new Point(188, 4);
            rbExpMenos6.Name = "rbExpMenos6";
            rbExpMenos6.Size = new Size(172, 27);
            rbExpMenos6.TabIndex = 1;
            rbExpMenos6.Tag = "Menos6Meses";
            rbExpMenos6.Text = "Menos de 6 meses";
            rbExpMenos6.UseVisualStyleBackColor = true;
            // 
            // rbExp1Ano
            // 
            rbExp1Ano.AutoSize = true;
            rbExp1Ano.Font = new Font("Segoe UI", 10F);
            rbExp1Ano.Location = new Point(378, 4);
            rbExp1Ano.Name = "rbExp1Ano";
            rbExp1Ano.Size = new Size(74, 27);
            rbExp1Ano.TabIndex = 2;
            rbExp1Ano.Tag = "1Ano";
            rbExp1Ano.Text = "1 año";
            rbExp1Ano.UseVisualStyleBackColor = true;
            // 
            // rbExp2Anos
            // 
            rbExp2Anos.AutoSize = true;
            rbExp2Anos.Font = new Font("Segoe UI", 10F);
            rbExp2Anos.Location = new Point(0, 36);
            rbExp2Anos.Name = "rbExp2Anos";
            rbExp2Anos.Size = new Size(81, 27);
            rbExp2Anos.TabIndex = 3;
            rbExp2Anos.Tag = "2Anos";
            rbExp2Anos.Text = "2 años";
            rbExp2Anos.UseVisualStyleBackColor = true;
            // 
            // rbExpMas5
            // 
            rbExpMas5.AutoSize = true;
            rbExpMas5.Font = new Font("Segoe UI", 10F);
            rbExpMas5.Location = new Point(188, 36);
            rbExpMas5.Name = "rbExpMas5";
            rbExpMas5.Size = new Size(141, 27);
            rbExpMas5.TabIndex = 4;
            rbExpMas5.Tag = "Mas5Anos";
            rbExpMas5.Text = "Más de 5 años";
            rbExpMas5.UseVisualStyleBackColor = true;
            // 
            // lblHorarioPreferido
            // 
            lblHorarioPreferido.AutoSize = true;
            lblHorarioPreferido.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblHorarioPreferido.ForeColor = Color.FromArgb(15, 23, 42);
            lblHorarioPreferido.Location = new Point(720, 1145);
            lblHorarioPreferido.Name = "lblHorarioPreferido";
            lblHorarioPreferido.Size = new Size(237, 30);
            lblHorarioPreferido.TabIndex = 75;
            lblHorarioPreferido.Text = "HORARIO PREFERIDO";
            // 
            // panelHorario
            // 
            panelHorario.BackColor = Color.Transparent;
            panelHorario.Controls.Add(rbHorManana);
            panelHorario.Controls.Add(rbHorTarde);
            panelHorario.Controls.Add(rbHorNoche);
            panelHorario.Controls.Add(rbHorVariado);
            panelHorario.Controls.Add(txtHorarioVariadoDetalle);
            panelHorario.Location = new Point(662, 1181);
            panelHorario.Name = "panelHorario";
            panelHorario.Size = new Size(560, 70);
            panelHorario.TabIndex = 76;
            // 
            // rbHorManana
            // 
            rbHorManana.AutoSize = true;
            rbHorManana.Font = new Font("Segoe UI", 10F);
            rbHorManana.Location = new Point(0, 4);
            rbHorManana.Name = "rbHorManana";
            rbHorManana.Size = new Size(93, 27);
            rbHorManana.TabIndex = 0;
            rbHorManana.Tag = "Manana";
            rbHorManana.Text = "Mañana";
            rbHorManana.UseVisualStyleBackColor = true;
            // 
            // rbHorTarde
            // 
            rbHorTarde.AutoSize = true;
            rbHorTarde.Font = new Font("Segoe UI", 10F);
            rbHorTarde.Location = new Point(120, 4);
            rbHorTarde.Name = "rbHorTarde";
            rbHorTarde.Size = new Size(72, 27);
            rbHorTarde.TabIndex = 1;
            rbHorTarde.Tag = "Tarde";
            rbHorTarde.Text = "Tarde";
            rbHorTarde.UseVisualStyleBackColor = true;
            // 
            // rbHorNoche
            // 
            rbHorNoche.AutoSize = true;
            rbHorNoche.Font = new Font("Segoe UI", 10F);
            rbHorNoche.Location = new Point(240, 4);
            rbHorNoche.Name = "rbHorNoche";
            rbHorNoche.Size = new Size(81, 27);
            rbHorNoche.TabIndex = 2;
            rbHorNoche.Tag = "Noche";
            rbHorNoche.Text = "Noche";
            rbHorNoche.UseVisualStyleBackColor = true;
            // 
            // rbHorVariado
            // 
            rbHorVariado.AutoSize = true;
            rbHorVariado.Font = new Font("Segoe UI", 10F);
            rbHorVariado.Location = new Point(0, 36);
            rbHorVariado.Name = "rbHorVariado";
            rbHorVariado.Size = new Size(89, 27);
            rbHorVariado.TabIndex = 3;
            rbHorVariado.Tag = "Variado";
            rbHorVariado.Text = "Variado";
            rbHorVariado.UseVisualStyleBackColor = true;
            // 
            // txtHorarioVariadoDetalle
            // 
            txtHorarioVariadoDetalle.Enabled = false;
            txtHorarioVariadoDetalle.Font = new Font("Segoe UI", 11F);
            txtHorarioVariadoDetalle.Location = new Point(90, 32);
            txtHorarioVariadoDetalle.Name = "txtHorarioVariadoDetalle";
            txtHorarioVariadoDetalle.PlaceholderText = "Explique su horario...";
            txtHorarioVariadoDetalle.Size = new Size(380, 32);
            txtHorarioVariadoDetalle.TabIndex = 4;
            // 
            // tabMiembros
            // 
            tabMiembros.BackColor = Color.White;
            tabMiembros.Controls.Add(layoutMiembros);
            tabMiembros.ForeColor = Color.FromArgb(15, 23, 42);
            tabMiembros.Location = new Point(4, 34);
            tabMiembros.Name = "tabMiembros";
            tabMiembros.Padding = new Padding(16);
            tabMiembros.Size = new Size(1505, 701);
            tabMiembros.TabIndex = 1;
            tabMiembros.Text = "MIEMBROS";
            // 
            // layoutMiembros
            // 
            layoutMiembros.BackColor = Color.White;
            layoutMiembros.ColumnCount = 1;
            layoutMiembros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutMiembros.Controls.Add(panelToolbarMiembros, 0, 0);
            layoutMiembros.Controls.Add(dgvClientes, 0, 1);
            layoutMiembros.Controls.Add(panelDetalleScroll, 0, 2);
            layoutMiembros.Dock = DockStyle.Fill;
            layoutMiembros.Location = new Point(16, 16);
            layoutMiembros.Margin = new Padding(0);
            layoutMiembros.Name = "layoutMiembros";
            layoutMiembros.RowCount = 3;
            layoutMiembros.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            layoutMiembros.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            layoutMiembros.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            layoutMiembros.Size = new Size(1473, 669);
            layoutMiembros.TabIndex = 0;
            // 
            // panelToolbarMiembros
            // 
            panelToolbarMiembros.BackColor = Color.White;
            panelToolbarMiembros.Controls.Add(label6);
            panelToolbarMiembros.Controls.Add(txtBuscar);
            panelToolbarMiembros.Dock = DockStyle.Fill;
            panelToolbarMiembros.Location = new Point(0, 0);
            panelToolbarMiembros.Margin = new Padding(0);
            panelToolbarMiembros.Name = "panelToolbarMiembros";
            panelToolbarMiembros.Size = new Size(1473, 64);
            panelToolbarMiembros.TabIndex = 0;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label6.ForeColor = Color.FromArgb(15, 23, 42);
            label6.Location = new Point(0, 18);
            label6.Name = "label6";
            label6.Size = new Size(77, 23);
            label6.TabIndex = 0;
            label6.Text = "BUSCAR";
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscar.Font = new Font("Segoe UI", 11F);
            txtBuscar.Location = new Point(90, 14);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar por nombre, telefono...";
            txtBuscar.Size = new Size(1000, 32);
            txtBuscar.TabIndex = 1;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // dgvClientes
            // 
            dgvClientes.AllowUserToAddRows = false;
            dgvClientes.AllowUserToDeleteRows = false;
            dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClientes.BackgroundColor = Color.White;
            dgvClientes.BorderStyle = BorderStyle.None;
            dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClientes.Dock = DockStyle.Fill;
            dgvClientes.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgvClientes.Location = new Point(0, 64);
            dgvClientes.Margin = new Padding(0, 0, 0, 12);
            dgvClientes.Name = "dgvClientes";
            dgvClientes.ReadOnly = true;
            dgvClientes.RowHeadersWidth = 51;
            dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClientes.Size = new Size(1473, 217);
            dgvClientes.TabIndex = 1;
            dgvClientes.CellClick += dgvClientes_CellClick;
            dgvClientes.CellDoubleClick += dgvClientes_CellDoubleClick;
            dgvClientes.SelectionChanged += dgvClientes_SelectionChanged;
            // 
            // panelDetalleScroll
            // 
            panelDetalleScroll.BackColor = Color.White;
            panelDetalleScroll.Controls.Add(ucFichaResumen);
            panelDetalleScroll.Dock = DockStyle.Fill;
            panelDetalleScroll.Location = new Point(0, 293);
            panelDetalleScroll.Margin = new Padding(0);
            panelDetalleScroll.Name = "panelDetalleScroll";
            panelDetalleScroll.Size = new Size(1473, 376);
            panelDetalleScroll.TabIndex = 2;
            // 
            // ucFichaResumen
            // 
            ucFichaResumen.AutoScroll = true;
            ucFichaResumen.BackColor = Color.White;
            ucFichaResumen.Dock = DockStyle.Fill;
            ucFichaResumen.Font = new Font("Segoe UI", 9F);
            ucFichaResumen.Location = new Point(0, 0);
            ucFichaResumen.Name = "ucFichaResumen";
            ucFichaResumen.Size = new Size(1473, 376);
            ucFichaResumen.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBack.Location = new Point(16, 12);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(80, 36);
            btnBack.TabIndex = 0;
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Visible = false;
            // 
            // FrmClientes
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoScroll = true;
            AutoScrollMinSize = new Size(1080, 720);
            BackColor = Color.White;
            ClientSize = new Size(1062, 673);
            Controls.Add(layoutNavClientes);
            Controls.Add(btnBack);
            Name = "FrmClientes";
            StartPosition = FormStartPosition.CenterParent;
            Text = "MIEMBROS";
            WindowState = FormWindowState.Maximized;
            Load += FrmClientes_Load;
            layoutNavClientes.ResumeLayout(false);
            panelNav.ResumeLayout(false);
            tabControlClientes.ResumeLayout(false);
            tabAgregar.ResumeLayout(false);
            tabAgregar.PerformLayout();
            panelMedicamentos.ResumeLayout(false);
            panelMedicamentos.PerformLayout();
            panelAlergias.ResumeLayout(false);
            panelAlergias.PerformLayout();
            panelCirugias.ResumeLayout(false);
            panelCirugias.PerformLayout();
            panelExperiencia.ResumeLayout(false);
            panelExperiencia.PerformLayout();
            panelHorario.ResumeLayout(false);
            panelHorario.PerformLayout();
            tabMiembros.ResumeLayout(false);
            layoutMiembros.ResumeLayout(false);
            panelToolbarMiembros.ResumeLayout(false);
            panelToolbarMiembros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClientes).EndInit();
            panelDetalleScroll.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutNavClientes;
        private Panel panelNav;
        private Button btnNavBack;
        private Button btnNavPagar;
        private Button btnNavDeudas;
        private Button btnNavEstado;
        private Button btnNavCaja;
        private Button btnNavHistorial;
        private Button btnNavInventario;
        private Button btnNavReportes;
        private Button btnNavClientes;
        private TabControl tabControlClientes;
        private TabPage tabAgregar;
        private TabPage tabMiembros;
        private TableLayoutPanel layoutMiembros;
        private Panel panelToolbarMiembros;
        private DataGridView dgvClientes;
        private Panel panelDetalleScroll;
        private UI.DISEÑO.Controles.UcFichaResumenMiembro ucFichaResumen;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox txtNombre;
        private TextBox txtDireccion;
        private TextBox txtTelefono;
        private Button btnAgregar;
        private Label label2;
        private TextBox txtId;
        private DateTimePicker txtFecha;
        private Label label6;
        private TextBox txtBuscar;
        private Button btnBack;
        private Label label7;
        private DateTimePicker dtpFechaIngreso;
        private Label lblEdad;
        private Label label12;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label label8;
        private TextBox txtEmergenciaNombre;
        private Label label14;
        private Label label13;
        private TextBox txtEmergenciaTelefonoAlt;
        private TextBox txtEmergenciaTelefono;
        private TextBox txtEmergenciaParentesco;
        private CheckBox chkAsma;
        private CheckBox chkHipertension;
        private CheckBox chkDiabetes;
        private Label label15;
        private CheckBox chkProblemasCardiacos;
        private CheckBox chkColesterolAlto;
        private CheckBox chkArtritis;
        private CheckBox chkHernia;
        private CheckBox chkEpilepsia;
        private CheckBox chkEmbarazo;
        private CheckBox chkNingunaEnfermedad;
        private Label lblEnfermedadOtra;
        private TextBox txtEnfermedadOtra;
        private Label lblLesiones;
        private CheckBox chkLesionHombro;
        private CheckBox chkLesionRodilla;
        private CheckBox chkLesionEspalda;
        private CheckBox chkLesionCuello;
        private CheckBox chkLesionTobillo;
        private CheckBox chkLesionCadera;
        private Label lblLesionDescripcion;
        private TextBox txtLesionDescripcion;
        private Label lblMedicamentos;
        private Panel panelMedicamentos;
        private RadioButton rbMedicamentosSi;
        private RadioButton rbMedicamentosNo;
        private Label lblListaMedicamentos;
        private TextBox txtListaMedicamentos;
        private Label lblAlergias;
        private Panel panelAlergias;
        private RadioButton rbAlergiasSi;
        private RadioButton rbAlergiasNo;
        private Label lblAlergiasDescripcion;
        private TextBox txtAlergiasDescripcion;
        private Label lblCirugias;
        private Panel panelCirugias;
        private RadioButton rbCirugiasSi;
        private RadioButton rbCirugiasNo;
        private Label lblCirugiasDescripcion;
        private TextBox txtCirugiasDescripcion;
        private Label lblCirugiasFecha;
        private DateTimePicker dtpCirugiasFecha;
        private Label lblCirugiaAntiguedad;
        private Label lblObjetivoFitness;
        private Label lblObjetivoHint;
        private CheckBox chkObjPerderGrasa;
        private CheckBox chkObjGanarMasa;
        private CheckBox chkObjTonificar;
        private CheckBox chkObjMejorarCondicion;
        private CheckBox chkObjRehabilitacion;
        private CheckBox chkObjSalud;
        private CheckBox chkObjCompetencia;
        private CheckBox chkObjOtro;
        private TextBox txtObjOtroDescripcion;
        private Label lblExperiencia;
        private Panel panelExperiencia;
        private RadioButton rbExpNunca;
        private RadioButton rbExpMenos6;
        private RadioButton rbExp1Ano;
        private RadioButton rbExp2Anos;
        private RadioButton rbExpMas5;
        private Label lblHorarioPreferido;
        private Panel panelHorario;
        private RadioButton rbHorManana;
        private RadioButton rbHorTarde;
        private RadioButton rbHorNoche;
        private RadioButton rbHorVariado;
        private TextBox txtHorarioVariadoDetalle;
        private Label label16;
        private ComboBox cmbsexo;
        
    }
}
