using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class ClienteFichaSaludDAL
    {
        private readonly DBHelper db = new DBHelper();
        private static bool _schemaReady;
        private static readonly object SchemaLock = new();

        public void EnsureSchema()
        {
            if (_schemaReady) return;
            lock (SchemaLock)
            {
                if (_schemaReady) return;

                db.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClienteFichaSalud')
BEGIN
    CREATE TABLE dbo.ClienteFichaSalud
    (
        ClienteId INT NOT NULL PRIMARY KEY
            CONSTRAINT FK_ClienteFichaSalud_Clientes
            REFERENCES dbo.Clientes(ID) ON DELETE CASCADE,
        EmergenciaNombre NVARCHAR(120) NULL,
        EmergenciaParentesco NVARCHAR(80) NULL,
        EmergenciaTelefono NVARCHAR(40) NULL,
        EmergenciaTelefonoAlt NVARCHAR(40) NULL,
        Diabetes BIT NOT NULL CONSTRAINT DF_CFS_Diabetes DEFAULT(0),
        Hipertension BIT NOT NULL CONSTRAINT DF_CFS_Hipertension DEFAULT(0),
        Asma BIT NOT NULL CONSTRAINT DF_CFS_Asma DEFAULT(0),
        ProblemasCardiacos BIT NOT NULL CONSTRAINT DF_CFS_Cardiacos DEFAULT(0),
        ColesterolAlto BIT NOT NULL CONSTRAINT DF_CFS_Colesterol DEFAULT(0),
        Artritis BIT NOT NULL CONSTRAINT DF_CFS_Artritis DEFAULT(0),
        Hernia BIT NOT NULL CONSTRAINT DF_CFS_Hernia DEFAULT(0),
        Epilepsia BIT NOT NULL CONSTRAINT DF_CFS_Epilepsia DEFAULT(0),
        Embarazo BIT NOT NULL CONSTRAINT DF_CFS_Embarazo DEFAULT(0),
        NingunaEnfermedad BIT NOT NULL CONSTRAINT DF_CFS_Ninguna DEFAULT(0),
        EnfermedadOtra NVARCHAR(200) NULL,
        LesionHombro BIT NOT NULL CONSTRAINT DF_CFS_Hombro DEFAULT(0),
        LesionRodilla BIT NOT NULL CONSTRAINT DF_CFS_Rodilla DEFAULT(0),
        LesionEspalda BIT NOT NULL CONSTRAINT DF_CFS_Espalda DEFAULT(0),
        LesionCuello BIT NOT NULL CONSTRAINT DF_CFS_Cuello DEFAULT(0),
        LesionTobillo BIT NOT NULL CONSTRAINT DF_CFS_Tobillo DEFAULT(0),
        LesionCadera BIT NOT NULL CONSTRAINT DF_CFS_Cadera DEFAULT(0),
        LesionDescripcion NVARCHAR(500) NULL,
        TomaMedicamentos BIT NOT NULL CONSTRAINT DF_CFS_TomaMeds DEFAULT(0),
        ListaMedicamentos NVARCHAR(500) NULL,
        TieneAlergias BIT NOT NULL CONSTRAINT DF_CFS_Alergias DEFAULT(0),
        AlergiasDescripcion NVARCHAR(500) NULL,
        TieneCirugias BIT NOT NULL CONSTRAINT DF_CFS_Cirugias DEFAULT(0),
        CirugiasDescripcion NVARCHAR(500) NULL,
        CirugiasFecha DATE NULL,
        ObjPerderGrasa BIT NOT NULL CONSTRAINT DF_CFS_ObjGrasa DEFAULT(0),
        ObjGanarMasa BIT NOT NULL CONSTRAINT DF_CFS_ObjMasa DEFAULT(0),
        ObjTonificar BIT NOT NULL CONSTRAINT DF_CFS_ObjToni DEFAULT(0),
        ObjMejorarCondicion BIT NOT NULL CONSTRAINT DF_CFS_ObjCond DEFAULT(0),
        ObjRehabilitacion BIT NOT NULL CONSTRAINT DF_CFS_ObjRehab DEFAULT(0),
        ObjSalud BIT NOT NULL CONSTRAINT DF_CFS_ObjSalud DEFAULT(0),
        ObjCompetencia BIT NOT NULL CONSTRAINT DF_CFS_ObjComp DEFAULT(0),
        ObjOtro BIT NOT NULL CONSTRAINT DF_CFS_ObjOtro DEFAULT(0),
        ObjOtroDescripcion NVARCHAR(200) NULL,
        ExperienciaNivel NVARCHAR(30) NULL,
        HorarioPreferido NVARCHAR(20) NULL,
        HorarioVariadoDetalle NVARCHAR(200) NULL,
        FechaIngreso DATE NULL,
        FechaActualizacion DATETIME2 NOT NULL CONSTRAINT DF_CFS_FechaAct DEFAULT(SYSDATETIME())
    );
END");

                AsegurarColumna("TieneAlergias", "BIT NOT NULL CONSTRAINT DF_CFS_Alergias DEFAULT(0)");
                AsegurarColumna("AlergiasDescripcion", "NVARCHAR(500) NULL");
                AsegurarColumna("TieneCirugias", "BIT NOT NULL CONSTRAINT DF_CFS_Cirugias DEFAULT(0)");
                AsegurarColumna("CirugiasDescripcion", "NVARCHAR(500) NULL");
                AsegurarColumna("CirugiasFecha", "DATE NULL");
                AsegurarColumna("ObjPerderGrasa", "BIT NOT NULL CONSTRAINT DF_CFS_ObjGrasa DEFAULT(0)");
                AsegurarColumna("ObjGanarMasa", "BIT NOT NULL CONSTRAINT DF_CFS_ObjMasa DEFAULT(0)");
                AsegurarColumna("ObjTonificar", "BIT NOT NULL CONSTRAINT DF_CFS_ObjToni DEFAULT(0)");
                AsegurarColumna("ObjMejorarCondicion", "BIT NOT NULL CONSTRAINT DF_CFS_ObjCond DEFAULT(0)");
                AsegurarColumna("ObjRehabilitacion", "BIT NOT NULL CONSTRAINT DF_CFS_ObjRehab DEFAULT(0)");
                AsegurarColumna("ObjSalud", "BIT NOT NULL CONSTRAINT DF_CFS_ObjSalud DEFAULT(0)");
                AsegurarColumna("ObjCompetencia", "BIT NOT NULL CONSTRAINT DF_CFS_ObjComp DEFAULT(0)");
                AsegurarColumna("ObjOtro", "BIT NOT NULL CONSTRAINT DF_CFS_ObjOtro DEFAULT(0)");
                AsegurarColumna("ObjOtroDescripcion", "NVARCHAR(200) NULL");
                AsegurarColumna("ExperienciaNivel", "NVARCHAR(30) NULL");
                AsegurarColumna("HorarioPreferido", "NVARCHAR(20) NULL");
                AsegurarColumna("HorarioVariadoDetalle", "NVARCHAR(200) NULL");

                _schemaReady = true;
            }
        }

        private void AsegurarColumna(string columna, string definicionSql)
        {
            db.ExecuteNonQuery($@"
IF COL_LENGTH('dbo.ClienteFichaSalud', '{columna}') IS NULL
    ALTER TABLE dbo.ClienteFichaSalud ADD {columna} {definicionSql};");
        }

        public void Guardar(ClienteFichaSaludDTO ficha)
        {
            if (ficha == null || ficha.ClienteId <= 0)
                throw new ArgumentException("Ficha de salud inválida.");

            EnsureSchema();

            const string query = @"
MERGE dbo.ClienteFichaSalud AS t
USING (SELECT @ClienteId AS ClienteId) AS s
ON t.ClienteId = s.ClienteId
WHEN MATCHED THEN UPDATE SET
    EmergenciaNombre = @EmergenciaNombre,
    EmergenciaParentesco = @EmergenciaParentesco,
    EmergenciaTelefono = @EmergenciaTelefono,
    EmergenciaTelefonoAlt = @EmergenciaTelefonoAlt,
    Diabetes = @Diabetes,
    Hipertension = @Hipertension,
    Asma = @Asma,
    ProblemasCardiacos = @ProblemasCardiacos,
    ColesterolAlto = @ColesterolAlto,
    Artritis = @Artritis,
    Hernia = @Hernia,
    Epilepsia = @Epilepsia,
    Embarazo = @Embarazo,
    NingunaEnfermedad = @NingunaEnfermedad,
    EnfermedadOtra = @EnfermedadOtra,
    LesionHombro = @LesionHombro,
    LesionRodilla = @LesionRodilla,
    LesionEspalda = @LesionEspalda,
    LesionCuello = @LesionCuello,
    LesionTobillo = @LesionTobillo,
    LesionCadera = @LesionCadera,
    LesionDescripcion = @LesionDescripcion,
    TomaMedicamentos = @TomaMedicamentos,
    ListaMedicamentos = @ListaMedicamentos,
    TieneAlergias = @TieneAlergias,
    AlergiasDescripcion = @AlergiasDescripcion,
    TieneCirugias = @TieneCirugias,
    CirugiasDescripcion = @CirugiasDescripcion,
    CirugiasFecha = @CirugiasFecha,
    ObjPerderGrasa = @ObjPerderGrasa,
    ObjGanarMasa = @ObjGanarMasa,
    ObjTonificar = @ObjTonificar,
    ObjMejorarCondicion = @ObjMejorarCondicion,
    ObjRehabilitacion = @ObjRehabilitacion,
    ObjSalud = @ObjSalud,
    ObjCompetencia = @ObjCompetencia,
    ObjOtro = @ObjOtro,
    ObjOtroDescripcion = @ObjOtroDescripcion,
    ExperienciaNivel = @ExperienciaNivel,
    HorarioPreferido = @HorarioPreferido,
    HorarioVariadoDetalle = @HorarioVariadoDetalle,
    FechaIngreso = @FechaIngreso,
    FechaActualizacion = SYSDATETIME()
WHEN NOT MATCHED THEN INSERT
(
    ClienteId, EmergenciaNombre, EmergenciaParentesco, EmergenciaTelefono, EmergenciaTelefonoAlt,
    Diabetes, Hipertension, Asma, ProblemasCardiacos, ColesterolAlto, Artritis, Hernia, Epilepsia, Embarazo,
    NingunaEnfermedad, EnfermedadOtra,
    LesionHombro, LesionRodilla, LesionEspalda, LesionCuello, LesionTobillo, LesionCadera, LesionDescripcion,
    TomaMedicamentos, ListaMedicamentos,
    TieneAlergias, AlergiasDescripcion, TieneCirugias, CirugiasDescripcion, CirugiasFecha,
    ObjPerderGrasa, ObjGanarMasa, ObjTonificar, ObjMejorarCondicion, ObjRehabilitacion, ObjSalud, ObjCompetencia, ObjOtro, ObjOtroDescripcion,
    ExperienciaNivel, HorarioPreferido, HorarioVariadoDetalle,
    FechaIngreso, FechaActualizacion
)
VALUES
(
    @ClienteId, @EmergenciaNombre, @EmergenciaParentesco, @EmergenciaTelefono, @EmergenciaTelefonoAlt,
    @Diabetes, @Hipertension, @Asma, @ProblemasCardiacos, @ColesterolAlto, @Artritis, @Hernia, @Epilepsia, @Embarazo,
    @NingunaEnfermedad, @EnfermedadOtra,
    @LesionHombro, @LesionRodilla, @LesionEspalda, @LesionCuello, @LesionTobillo, @LesionCadera, @LesionDescripcion,
    @TomaMedicamentos, @ListaMedicamentos,
    @TieneAlergias, @AlergiasDescripcion, @TieneCirugias, @CirugiasDescripcion, @CirugiasFecha,
    @ObjPerderGrasa, @ObjGanarMasa, @ObjTonificar, @ObjMejorarCondicion, @ObjRehabilitacion, @ObjSalud, @ObjCompetencia, @ObjOtro, @ObjOtroDescripcion,
    @ExperienciaNivel, @HorarioPreferido, @HorarioVariadoDetalle,
    @FechaIngreso, SYSDATETIME()
);";

            db.ExecuteNonQuery(query, CrearParametros(ficha));
        }

        public ClienteFichaSaludDTO? ObtenerPorClienteId(int clienteId)
        {
            EnsureSchema();

            string query = @"
SELECT *
FROM dbo.ClienteFichaSalud
WHERE ClienteId = @ClienteId";

            DataTable dt = db.ExecuteQuery(query, new[] { new SqlParameter("@ClienteId", clienteId) });
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new ClienteFichaSaludDTO
            {
                ClienteId = clienteId,
                EmergenciaNombre = r["EmergenciaNombre"]?.ToString(),
                EmergenciaParentesco = r["EmergenciaParentesco"]?.ToString(),
                EmergenciaTelefono = r["EmergenciaTelefono"]?.ToString(),
                EmergenciaTelefonoAlt = r["EmergenciaTelefonoAlt"]?.ToString(),
                Diabetes = Bit(r, "Diabetes"),
                Hipertension = Bit(r, "Hipertension"),
                Asma = Bit(r, "Asma"),
                ProblemasCardiacos = Bit(r, "ProblemasCardiacos"),
                ColesterolAlto = Bit(r, "ColesterolAlto"),
                Artritis = Bit(r, "Artritis"),
                Hernia = Bit(r, "Hernia"),
                Epilepsia = Bit(r, "Epilepsia"),
                Embarazo = Bit(r, "Embarazo"),
                NingunaEnfermedad = Bit(r, "NingunaEnfermedad"),
                EnfermedadOtra = r["EnfermedadOtra"]?.ToString(),
                LesionHombro = Bit(r, "LesionHombro"),
                LesionRodilla = Bit(r, "LesionRodilla"),
                LesionEspalda = Bit(r, "LesionEspalda"),
                LesionCuello = Bit(r, "LesionCuello"),
                LesionTobillo = Bit(r, "LesionTobillo"),
                LesionCadera = Bit(r, "LesionCadera"),
                LesionDescripcion = r["LesionDescripcion"]?.ToString(),
                TomaMedicamentos = Bit(r, "TomaMedicamentos"),
                ListaMedicamentos = r["ListaMedicamentos"]?.ToString(),
                TieneAlergias = Bit(r, "TieneAlergias"),
                AlergiasDescripcion = Col(r, "AlergiasDescripcion"),
                TieneCirugias = Bit(r, "TieneCirugias"),
                CirugiasDescripcion = Col(r, "CirugiasDescripcion"),
                CirugiasFecha = DateOrNull(r, "CirugiasFecha"),
                ObjPerderGrasa = Bit(r, "ObjPerderGrasa"),
                ObjGanarMasa = Bit(r, "ObjGanarMasa"),
                ObjTonificar = Bit(r, "ObjTonificar"),
                ObjMejorarCondicion = Bit(r, "ObjMejorarCondicion"),
                ObjRehabilitacion = Bit(r, "ObjRehabilitacion"),
                ObjSalud = Bit(r, "ObjSalud"),
                ObjCompetencia = Bit(r, "ObjCompetencia"),
                ObjOtro = Bit(r, "ObjOtro"),
                ObjOtroDescripcion = Col(r, "ObjOtroDescripcion"),
                ExperienciaNivel = Col(r, "ExperienciaNivel"),
                HorarioPreferido = Col(r, "HorarioPreferido"),
                HorarioVariadoDetalle = Col(r, "HorarioVariadoDetalle"),
                FechaIngreso = DateOrNull(r, "FechaIngreso")
            };
        }

        private static bool Bit(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value && Convert.ToBoolean(r[col]);

        private static string? Col(DataRow r, string col) =>
            r.Table.Columns.Contains(col) ? r[col]?.ToString() : null;

        private static DateTime? DateOrNull(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value
                ? Convert.ToDateTime(r[col])
                : null;

        private static SqlParameter[] CrearParametros(ClienteFichaSaludDTO f) =>
        [
            new SqlParameter("@ClienteId", f.ClienteId),
            new SqlParameter("@EmergenciaNombre", (object?)NullIfEmpty(f.EmergenciaNombre) ?? DBNull.Value),
            new SqlParameter("@EmergenciaParentesco", (object?)NullIfEmpty(f.EmergenciaParentesco) ?? DBNull.Value),
            new SqlParameter("@EmergenciaTelefono", (object?)NullIfEmpty(f.EmergenciaTelefono) ?? DBNull.Value),
            new SqlParameter("@EmergenciaTelefonoAlt", (object?)NullIfEmpty(f.EmergenciaTelefonoAlt) ?? DBNull.Value),
            new SqlParameter("@Diabetes", f.Diabetes),
            new SqlParameter("@Hipertension", f.Hipertension),
            new SqlParameter("@Asma", f.Asma),
            new SqlParameter("@ProblemasCardiacos", f.ProblemasCardiacos),
            new SqlParameter("@ColesterolAlto", f.ColesterolAlto),
            new SqlParameter("@Artritis", f.Artritis),
            new SqlParameter("@Hernia", f.Hernia),
            new SqlParameter("@Epilepsia", f.Epilepsia),
            new SqlParameter("@Embarazo", f.Embarazo),
            new SqlParameter("@NingunaEnfermedad", f.NingunaEnfermedad),
            new SqlParameter("@EnfermedadOtra", (object?)NullIfEmpty(f.EnfermedadOtra) ?? DBNull.Value),
            new SqlParameter("@LesionHombro", f.LesionHombro),
            new SqlParameter("@LesionRodilla", f.LesionRodilla),
            new SqlParameter("@LesionEspalda", f.LesionEspalda),
            new SqlParameter("@LesionCuello", f.LesionCuello),
            new SqlParameter("@LesionTobillo", f.LesionTobillo),
            new SqlParameter("@LesionCadera", f.LesionCadera),
            new SqlParameter("@LesionDescripcion", (object?)NullIfEmpty(f.LesionDescripcion) ?? DBNull.Value),
            new SqlParameter("@TomaMedicamentos", f.TomaMedicamentos),
            new SqlParameter("@ListaMedicamentos", (object?)NullIfEmpty(f.ListaMedicamentos) ?? DBNull.Value),
            new SqlParameter("@TieneAlergias", f.TieneAlergias),
            new SqlParameter("@AlergiasDescripcion", (object?)NullIfEmpty(f.AlergiasDescripcion) ?? DBNull.Value),
            new SqlParameter("@TieneCirugias", f.TieneCirugias),
            new SqlParameter("@CirugiasDescripcion", (object?)NullIfEmpty(f.CirugiasDescripcion) ?? DBNull.Value),
            new SqlParameter("@CirugiasFecha", (object?)f.CirugiasFecha?.Date ?? DBNull.Value),
            new SqlParameter("@ObjPerderGrasa", f.ObjPerderGrasa),
            new SqlParameter("@ObjGanarMasa", f.ObjGanarMasa),
            new SqlParameter("@ObjTonificar", f.ObjTonificar),
            new SqlParameter("@ObjMejorarCondicion", f.ObjMejorarCondicion),
            new SqlParameter("@ObjRehabilitacion", f.ObjRehabilitacion),
            new SqlParameter("@ObjSalud", f.ObjSalud),
            new SqlParameter("@ObjCompetencia", f.ObjCompetencia),
            new SqlParameter("@ObjOtro", f.ObjOtro),
            new SqlParameter("@ObjOtroDescripcion", (object?)NullIfEmpty(f.ObjOtroDescripcion) ?? DBNull.Value),
            new SqlParameter("@ExperienciaNivel", (object?)NullIfEmpty(f.ExperienciaNivel) ?? DBNull.Value),
            new SqlParameter("@HorarioPreferido", (object?)NullIfEmpty(f.HorarioPreferido) ?? DBNull.Value),
            new SqlParameter("@HorarioVariadoDetalle", (object?)NullIfEmpty(f.HorarioVariadoDetalle) ?? DBNull.Value),
            new SqlParameter("@FechaIngreso", (object?)f.FechaIngreso?.Date ?? DBNull.Value)
        ];

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
