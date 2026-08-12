using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class CongelacionDAL
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
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CongelacionesMembresia')
BEGIN
    CREATE TABLE dbo.CongelacionesMembresia
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL
            CONSTRAINT FK_Congelaciones_Clientes
            REFERENCES dbo.Clientes(ID),
        MembresiaId INT NULL,
        FechaCongelacion DATE NOT NULL,
        DiaAncla INT NOT NULL,
        DiasRestantes INT NOT NULL,
        FechaFinOriginal DATE NULL,
        Motivo NVARCHAR(300) NOT NULL,
        Usuario NVARCHAR(80) NULL,
        FechaActivacion DATE NULL,
        Activa BIT NOT NULL CONSTRAINT DF_Congelaciones_Activa DEFAULT(1)
    );

    CREATE INDEX IX_Congelaciones_Cliente_Activa
        ON dbo.CongelacionesMembresia (ClienteId, Activa);
END");

                _schemaReady = true;
            }
        }

        public CongelacionDTO? ObtenerActiva(int clienteId)
        {
            EnsureSchema();
            string query = @"
                SELECT TOP 1 *
                FROM CongelacionesMembresia
                WHERE ClienteId = @ClienteId AND Activa = 1
                ORDER BY Id DESC";

            var dt = db.ExecuteQuery(query, new[] { new SqlParameter("@ClienteId", clienteId) });
            if (dt.Rows.Count == 0)
                return null;

            return Mapear(dt.Rows[0]);
        }

        public int Insertar(CongelacionDTO dto)
        {
            EnsureSchema();
            string query = @"
                INSERT INTO CongelacionesMembresia
                    (ClienteId, MembresiaId, FechaCongelacion, DiaAncla, DiasRestantes,
                     FechaFinOriginal, Motivo, Usuario, Activa)
                OUTPUT INSERTED.Id
                VALUES
                    (@ClienteId, @MembresiaId, @FechaCongelacion, @DiaAncla, @DiasRestantes,
                     @FechaFinOriginal, @Motivo, @Usuario, 1)";

            SqlParameter[] p =
            {
                new SqlParameter("@ClienteId", dto.ClienteId),
                new SqlParameter("@MembresiaId", (object?)dto.MembresiaId ?? DBNull.Value),
                new SqlParameter("@FechaCongelacion", dto.FechaCongelacion.Date),
                new SqlParameter("@DiaAncla", dto.DiaAncla),
                new SqlParameter("@DiasRestantes", dto.DiasRestantes),
                new SqlParameter("@FechaFinOriginal", (object?)dto.FechaFinOriginal ?? DBNull.Value),
                new SqlParameter("@Motivo", dto.Motivo ?? ""),
                new SqlParameter("@Usuario", (object?)dto.Usuario ?? "ADMIN")
            };

            return Convert.ToInt32(db.ExecuteScalar(query, p));
        }

        public void CerrarActiva(int clienteId, DateTime? fechaActivacion)
        {
            EnsureSchema();
            string query = @"
                UPDATE CongelacionesMembresia
                SET Activa = 0,
                    FechaActivacion = @FechaActivacion
                WHERE ClienteId = @ClienteId AND Activa = 1";

            db.ExecuteNonQuery(query, new[]
            {
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@FechaActivacion", (object?)fechaActivacion?.Date ?? DBNull.Value)
            });
        }

        public void CerrarActiva(SqlConnection conn, SqlTransaction tx, int clienteId, DateTime? fechaActivacion)
        {
            using var cmd = new SqlCommand(@"
                IF OBJECT_ID('dbo.CongelacionesMembresia', 'U') IS NOT NULL
                BEGIN
                    UPDATE CongelacionesMembresia
                    SET Activa = 0,
                        FechaActivacion = @FechaActivacion
                    WHERE ClienteId = @ClienteId AND Activa = 1
                END", conn, tx);
            cmd.Parameters.AddWithValue("@ClienteId", clienteId);
            cmd.Parameters.AddWithValue("@FechaActivacion", (object?)fechaActivacion?.Date ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static CongelacionDTO Mapear(DataRow row)
        {
            return new CongelacionDTO
            {
                Id = Convert.ToInt32(row["Id"]),
                ClienteId = Convert.ToInt32(row["ClienteId"]),
                MembresiaId = row["MembresiaId"] == DBNull.Value ? null : Convert.ToInt32(row["MembresiaId"]),
                FechaCongelacion = Convert.ToDateTime(row["FechaCongelacion"]),
                DiaAncla = Convert.ToInt32(row["DiaAncla"]),
                DiasRestantes = Convert.ToInt32(row["DiasRestantes"]),
                FechaFinOriginal = row["FechaFinOriginal"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(row["FechaFinOriginal"]),
                Motivo = row["Motivo"]?.ToString() ?? "",
                Usuario = row["Usuario"]?.ToString() ?? "",
                FechaActivacion = row["FechaActivacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(row["FechaActivacion"]),
                Activa = Convert.ToBoolean(row["Activa"])
            };
        }
    }
}
