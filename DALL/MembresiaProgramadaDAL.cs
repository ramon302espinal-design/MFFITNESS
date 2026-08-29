using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DL
{
    public class MembresiaProgramadaDAL
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
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'MembresiasProgramadas')
BEGIN
    CREATE TABLE dbo.MembresiasProgramadas
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClienteId INT NOT NULL
            CONSTRAINT FK_MembProg_Clientes REFERENCES dbo.Clientes(ID),
        PlanId INT NOT NULL
            CONSTRAINT FK_MembProg_Planes REFERENCES dbo.Planes(Id),
        Monto DECIMAL(18,2) NOT NULL,
        FechaPago DATETIME NOT NULL,
        FechaInicioProgramada DATE NOT NULL,
        FechaFinProgramada DATE NOT NULL,
        MembresiaOrigenId INT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_MembProg_Estado DEFAULT(N'PENDIENTE'),
        Usuario NVARCHAR(80) NULL,
        FechaRegistro DATETIME NOT NULL
            CONSTRAINT DF_MembProg_FechaReg DEFAULT(GETDATE()),
        PagoId INT NULL,
        CajaMovimientoId INT NULL,
        Nota NVARCHAR(400) NULL
    );

    CREATE INDEX IX_MembProg_Cliente_Estado
        ON dbo.MembresiasProgramadas (ClienteId, Estado);

    CREATE INDEX IX_MembProg_Inicio_Estado
        ON dbo.MembresiasProgramadas (FechaInicioProgramada, Estado);
END");

                _schemaReady = true;
            }
        }

        public bool TienePendiente(int clienteId)
        {
            EnsureSchema();
            string query = @"
                SELECT COUNT(*)
                FROM MembresiasProgramadas
                WHERE ClienteId = @ClienteId
                  AND Estado = N'PENDIENTE'";

            return Convert.ToInt32(db.ExecuteScalar(query, new[]
            {
                new SqlParameter("@ClienteId", clienteId)
            })) > 0;
        }

        public MembresiaProgramadaDTO? ObtenerPendiente(int clienteId)
        {
            EnsureSchema();
            string query = @"
                SELECT TOP 1 mp.*, p.Nombre AS PlanNombre, c.Nombre AS ClienteNombre
                FROM MembresiasProgramadas mp
                INNER JOIN Planes p ON p.Id = mp.PlanId
                INNER JOIN Clientes c ON c.ID = mp.ClienteId
                WHERE mp.ClienteId = @ClienteId
                  AND mp.Estado = N'PENDIENTE'
                ORDER BY mp.Id DESC";

            var dt = db.ExecuteQuery(query, new[] { new SqlParameter("@ClienteId", clienteId) });
            return dt.Rows.Count == 0 ? null : Mapear(dt.Rows[0]);
        }

        public List<MembresiaProgramadaDTO> ListarPendientesParaAplicar(DateTime hastaInclusive)
        {
            EnsureSchema();
            string query = @"
                SELECT mp.*, p.Nombre AS PlanNombre, c.Nombre AS ClienteNombre
                FROM MembresiasProgramadas mp
                INNER JOIN Planes p ON p.Id = mp.PlanId
                INNER JOIN Clientes c ON c.ID = mp.ClienteId
                WHERE mp.Estado = N'PENDIENTE'
                  AND mp.FechaInicioProgramada <= @Hasta
                ORDER BY mp.FechaInicioProgramada, mp.Id";

            var dt = db.ExecuteQuery(query, new[]
            {
                new SqlParameter("@Hasta", hastaInclusive.Date)
            });

            var lista = new List<MembresiaProgramadaDTO>();
            foreach (DataRow row in dt.Rows)
                lista.Add(Mapear(row));
            return lista;
        }

        public int Insertar(SqlConnection conn, SqlTransaction tx, MembresiaProgramadaDTO dto)
        {
            EnsureSchema();
            string query = @"
                INSERT INTO MembresiasProgramadas
                    (ClienteId, PlanId, Monto, FechaPago, FechaInicioProgramada, FechaFinProgramada,
                     MembresiaOrigenId, Estado, Usuario, PagoId, CajaMovimientoId, Nota)
                OUTPUT INSERTED.Id
                VALUES
                    (@ClienteId, @PlanId, @Monto, @FechaPago, @FechaInicioProgramada, @FechaFinProgramada,
                     @MembresiaOrigenId, @Estado, @Usuario, @PagoId, @CajaMovimientoId, @Nota)";

            using var cmd = new SqlCommand(query, conn, tx);
            cmd.Parameters.AddWithValue("@ClienteId", dto.ClienteId);
            cmd.Parameters.AddWithValue("@PlanId", dto.PlanId);
            cmd.Parameters.AddWithValue("@Monto", dto.Monto);
            cmd.Parameters.AddWithValue("@FechaPago", dto.FechaPago);
            cmd.Parameters.AddWithValue("@FechaInicioProgramada", dto.FechaInicioProgramada.Date);
            cmd.Parameters.AddWithValue("@FechaFinProgramada", dto.FechaFinProgramada.Date);
            cmd.Parameters.AddWithValue("@MembresiaOrigenId", (object?)dto.MembresiaOrigenId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", dto.Estado ?? "PENDIENTE");
            cmd.Parameters.AddWithValue("@Usuario", (object?)dto.Usuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PagoId", (object?)dto.PagoId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CajaMovimientoId", (object?)dto.CajaMovimientoId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nota", (object?)dto.Nota ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void MarcarAplicada(SqlConnection conn, SqlTransaction tx, int id, int membresiaNuevaId)
        {
            EnsureSchema();
            string query = @"
                UPDATE MembresiasProgramadas
                SET Estado = N'APLICADA',
                    Nota = ISNULL(Nota, N'') + N' | Aplicada membresía #' + CAST(@MembresiaId AS NVARCHAR(20))
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, conn, tx);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@MembresiaId", membresiaNuevaId);
            cmd.ExecuteNonQuery();
        }

        public bool WhatsAppActivacionYaEnviada(int programacionId)
        {
            EnsureSchema();
            if (!ColumnExists("WhatsAppActivacionEnviada"))
                return false;

            string query = @"
                SELECT WhatsAppActivacionEnviada
                FROM MembresiasProgramadas
                WHERE Id = @Id";

            object? val = db.ExecuteScalar(query, new[] { new SqlParameter("@Id", programacionId) });
            return val != null && val != DBNull.Value && Convert.ToBoolean(val);
        }

        public void MarcarWhatsAppActivacionEnviada(int programacionId)
        {
            EnsureSchema();
            if (!ColumnExists("WhatsAppActivacionEnviada"))
                return;

            string query = @"
                UPDATE MembresiasProgramadas
                SET WhatsAppActivacionEnviada = 1
                WHERE Id = @Id";

            db.ExecuteNonQuery(query, new[] { new SqlParameter("@Id", programacionId) });
        }

        private bool ColumnExists(string columnName)
        {
            object? val = db.ExecuteScalar(@"
                SELECT CASE WHEN COL_LENGTH(N'dbo.MembresiasProgramadas', @Col) IS NULL THEN 0 ELSE 1 END",
                new[] { new SqlParameter("@Col", columnName) });
            return Convert.ToInt32(val) == 1;
        }

        private static MembresiaProgramadaDTO Mapear(DataRow row)
        {
            return new MembresiaProgramadaDTO
            {
                Id = Convert.ToInt32(row["Id"]),
                ClienteId = Convert.ToInt32(row["ClienteId"]),
                PlanId = Convert.ToInt32(row["PlanId"]),
                Monto = Convert.ToDecimal(row["Monto"]),
                FechaPago = Convert.ToDateTime(row["FechaPago"]),
                FechaInicioProgramada = Convert.ToDateTime(row["FechaInicioProgramada"]).Date,
                FechaFinProgramada = Convert.ToDateTime(row["FechaFinProgramada"]).Date,
                MembresiaOrigenId = row["MembresiaOrigenId"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(row["MembresiaOrigenId"]),
                Estado = row["Estado"]?.ToString() ?? "PENDIENTE",
                Usuario = row["Usuario"]?.ToString(),
                FechaRegistro = Convert.ToDateTime(row["FechaRegistro"]),
                PagoId = row["PagoId"] == DBNull.Value ? null : Convert.ToInt32(row["PagoId"]),
                CajaMovimientoId = row["CajaMovimientoId"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(row["CajaMovimientoId"]),
                Nota = row["Nota"]?.ToString(),
                PlanNombre = row.Table.Columns.Contains("PlanNombre")
                    ? row["PlanNombre"]?.ToString()
                    : null,
                ClienteNombre = row.Table.Columns.Contains("ClienteNombre")
                    ? row["ClienteNombre"]?.ToString()
                    : null
            };
        }
    }
}
