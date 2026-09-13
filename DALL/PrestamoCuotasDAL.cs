using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DL
{
    /// <summary>
    /// Cabecera + detalle de préstamos con plazos/interés/mora (pnlIntereses / dgvPlazos).
    /// </summary>
    public class PrestamoCuotasDAL
    {
        private readonly DBHelper db = new();
        private static bool _schemaReady;
        private static readonly object SchemaLock = new();

        public void EnsureSchema()
        {
            if (_schemaReady)
                return;

            lock (SchemaLock)
            {
                if (_schemaReady)
                    return;

                // Mismo DDL idempotente que migración 0039.
                db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.PrestamosCuotas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PrestamosCuotas
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        DeudaId             INT NOT NULL,
        ClienteId           INT NOT NULL,
        Frecuencia          NVARCHAR(20) NOT NULL,
        InteresPorcentaje   DECIMAL(9,4) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_InteresPorcentaje DEFAULT (0),
        NumeroPlazos        INT NOT NULL,
        InteresTotal        DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_InteresTotal DEFAULT (0),
        TotalConInteres     DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_TotalConInteres DEFAULT (0),
        CuotaBase           DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_CuotaBase DEFAULT (0),
        ActivarMora         BIT NOT NULL
            CONSTRAINT DF_PrestamosCuotas_ActivarMora DEFAULT (0),
        FechaCreacion       DATETIME2(0) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_FechaCreacion DEFAULT (SYSDATETIME()),
        Usuario             NVARCHAR(100) NULL,
        Estado              NVARCHAR(20) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_Estado DEFAULT (N'ACTIVO'),
        CONSTRAINT PK_PrestamosCuotas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_PrestamosCuotas_DeudaId UNIQUE (DeudaId),
        CONSTRAINT FK_PrestamosCuotas_Deudas
            FOREIGN KEY (DeudaId) REFERENCES dbo.Deudas (Id) ON DELETE CASCADE,
        CONSTRAINT FK_PrestamosCuotas_Clientes
            FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ID),
        CONSTRAINT CK_PrestamosCuotas_Frecuencia
            CHECK (Frecuencia IN (N'SEMANAL', N'QUINCENAL', N'MENSUAL')),
        CONSTRAINT CK_PrestamosCuotas_NumeroPlazos
            CHECK (NumeroPlazos > 0),
        CONSTRAINT CK_PrestamosCuotas_Montos
            CHECK (InteresPorcentaje >= 0
               AND InteresTotal >= 0
               AND TotalConInteres >= 0
               AND CuotaBase >= 0),
        CONSTRAINT CK_PrestamosCuotas_Estado
            CHECK (Estado IN (N'ACTIVO', N'CANCELADO', N'LIQUIDADO'))
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_PrestamosCuotas_ClienteId'
      AND object_id = OBJECT_ID(N'dbo.PrestamosCuotas'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PrestamosCuotas_ClienteId
        ON dbo.PrestamosCuotas (ClienteId);
END

IF OBJECT_ID(N'dbo.PrestamoDetalleCuotas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PrestamoDetalleCuotas
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        PrestamoId          INT NOT NULL,
        NumeroCuota         INT NOT NULL,
        FechaVencimiento    DATE NOT NULL,
        Capital             DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Capital DEFAULT (0),
        Interes             DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Interes DEFAULT (0),
        MoraPotencial       DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_MoraPotencial DEFAULT (0),
        Total               DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Total DEFAULT (0),
        CONSTRAINT PK_PrestamoDetalleCuotas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_PrestamoDetalleCuotas_Prestamo_Cuota
            UNIQUE (PrestamoId, NumeroCuota),
        CONSTRAINT FK_PrestamoDetalleCuotas_PrestamosCuotas
            FOREIGN KEY (PrestamoId) REFERENCES dbo.PrestamosCuotas (Id) ON DELETE CASCADE,
        CONSTRAINT CK_PrestamoDetalleCuotas_NumeroCuota
            CHECK (NumeroCuota > 0),
        CONSTRAINT CK_PrestamoDetalleCuotas_Montos
            CHECK (Capital >= 0
               AND Interes >= 0
               AND MoraPotencial >= 0
               AND Total >= 0)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_PrestamoDetalleCuotas_FechaVencimiento'
      AND object_id = OBJECT_ID(N'dbo.PrestamoDetalleCuotas'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PrestamoDetalleCuotas_FechaVencimiento
        ON dbo.PrestamoDetalleCuotas (FechaVencimiento);
END
");

                _schemaReady = true;
            }
        }

        public int InsertarPrestamoConDetalle(
            int deudaId,
            int clienteId,
            string frecuencia,
            decimal interesPorcentaje,
            int numeroPlazos,
            decimal interesTotal,
            decimal totalConInteres,
            decimal cuotaBase,
            bool activarMora,
            string? usuario,
            IReadOnlyList<PrestamoCuotaDetalleRow> cuotas)
        {
            if (deudaId <= 0)
                throw new InvalidOperationException("Deuda inválida.");
            if (clienteId <= 0)
                throw new InvalidOperationException("Cliente inválido.");
            if (cuotas == null || cuotas.Count == 0)
                throw new InvalidOperationException("El cronograma de cuotas está vacío.");
            if (cuotas.Count != numeroPlazos)
                throw new InvalidOperationException("El número de plazos no coincide con el detalle.");

            EnsureSchema();

            using SqlConnection conn = db.GetConnection();
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                // Ledger coherente con Precio Total (producto + interés):
                // MontoTotal = pagoInicial + TotalConInteres
                // MontoPagado = aporte inicial (si aún no estaba en ledger; InsertarDeuda lo deja en 0)
                // Saldo      = MontoTotal − MontoPagado  (= cronograma pendiente)
                decimal pagoInicial = 0m;
                decimal montoPagadoActual = 0m;
                using (var cmdPi = new SqlCommand(@"
SELECT
    CAST(CASE WHEN ISNULL(d.MontoPagado, 0) < 0 THEN 0 ELSE ISNULL(d.MontoPagado, 0) END AS DECIMAL(18,2)) AS MontoPagado,
    CAST(ISNULL((
        SELECT SUM(CASE
            WHEN h.TipoMovimiento = N'PAGO_INICIAL' THEN h.Monto
            WHEN h.TipoMovimiento = N'REVERSO_PAGO_INICIAL' THEN -h.Monto
            ELSE 0 END)
        FROM dbo.HistorialDeudas h
        WHERE h.DeudaId = d.Id
    ), 0) AS DECIMAL(18,2)) AS PagoInicial
FROM dbo.Deudas d
WHERE d.Id = @DeudaId
  AND d.Estado = N'ACTIVA';", conn, tx))
                {
                    cmdPi.Parameters.AddWithValue("@DeudaId", deudaId);
                    using var rd = cmdPi.ExecuteReader();
                    if (!rd.Read())
                        throw new InvalidOperationException("No se pudo ajustar la deuda con el interés del préstamo.");

                    montoPagadoActual = rd["MontoPagado"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(rd["MontoPagado"]);
                    pagoInicial = rd["PagoInicial"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(rd["PagoInicial"]);
                    if (pagoInicial < 0m)
                        pagoInicial = 0m;
                }

                decimal nuevoMontoTotal = decimal.Round(pagoInicial + totalConInteres, 2, MidpointRounding.AwayFromZero);
                // Si el ledger aún no reflejaba el aporte inicial (caso típico post-InsertarDeuda), incorpóralo.
                decimal nuevoMontoPagado = montoPagadoActual > 0m
                    ? montoPagadoActual
                    : pagoInicial;
                if (nuevoMontoPagado > nuevoMontoTotal)
                    nuevoMontoPagado = nuevoMontoTotal;

                using (var cmdUpd = new SqlCommand(@"
UPDATE dbo.Deudas
SET MontoTotal = @MontoTotal,
    MontoPagado = @MontoPagado,
    Saldo = CASE
                WHEN @MontoTotal > @MontoPagado THEN @MontoTotal - @MontoPagado
                ELSE 0
            END,
    Estado = CASE
                WHEN @MontoTotal > @MontoPagado THEN N'ACTIVA'
                ELSE N'PAGADA'
            END,
    FechaVencimiento = @Vence
WHERE Id = @DeudaId
  AND Estado = N'ACTIVA';", conn, tx))
                {
                    cmdUpd.Parameters.AddWithValue("@MontoTotal", nuevoMontoTotal);
                    cmdUpd.Parameters.AddWithValue("@MontoPagado", nuevoMontoPagado);
                    cmdUpd.Parameters.AddWithValue("@Vence", cuotas[^1].FechaVencimiento.Date);
                    cmdUpd.Parameters.AddWithValue("@DeudaId", deudaId);
                    int filas = cmdUpd.ExecuteNonQuery();
                    if (filas <= 0)
                        throw new InvalidOperationException("No se pudo ajustar la deuda con el interés del préstamo.");
                }

                using (var cmdHist = new SqlCommand(@"
INSERT INTO dbo.HistorialDeudas
(DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
VALUES
(@DeudaId, @ClienteId, N'PRESTAMO_PLAZOS', @Monto, @Descripcion, GETDATE(), @Usuario);", conn, tx))
                {
                    cmdHist.Parameters.AddWithValue("@DeudaId", deudaId);
                    cmdHist.Parameters.AddWithValue("@ClienteId", clienteId);
                    cmdHist.Parameters.AddWithValue("@Monto", nuevoMontoTotal);
                    cmdHist.Parameters.AddWithValue("@Descripcion",
                        $"Cronograma {frecuencia}: {numeroPlazos} plazo(s), interés fijo {interesPorcentaje:N2}%, " +
                        $"interés ${interesTotal:N2}, cronograma ${totalConInteres:N2}, " +
                        $"precio total ${nuevoMontoTotal:N2}" +
                        (activarMora ? ", mora activa ($50/día desde día 3)" : string.Empty));
                    cmdHist.Parameters.AddWithValue("@Usuario", (object?)usuario ?? DBNull.Value);
                    cmdHist.ExecuteNonQuery();
                }

                int prestamoId;
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.PrestamosCuotas
(DeudaId, ClienteId, Frecuencia, InteresPorcentaje, NumeroPlazos,
 InteresTotal, TotalConInteres, CuotaBase, ActivarMora, Usuario, Estado)
OUTPUT INSERTED.Id
VALUES
(@DeudaId, @ClienteId, @Frecuencia, @InteresPorcentaje, @NumeroPlazos,
 @InteresTotal, @TotalConInteres, @CuotaBase, @ActivarMora, @Usuario, N'ACTIVO');", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@DeudaId", deudaId);
                    cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                    cmd.Parameters.AddWithValue("@Frecuencia", frecuencia);
                    cmd.Parameters.AddWithValue("@InteresPorcentaje", interesPorcentaje);
                    cmd.Parameters.AddWithValue("@NumeroPlazos", numeroPlazos);
                    cmd.Parameters.AddWithValue("@InteresTotal", interesTotal);
                    cmd.Parameters.AddWithValue("@TotalConInteres", totalConInteres);
                    cmd.Parameters.AddWithValue("@CuotaBase", cuotaBase);
                    cmd.Parameters.AddWithValue("@ActivarMora", activarMora);
                    cmd.Parameters.AddWithValue("@Usuario", (object?)usuario ?? DBNull.Value);
                    prestamoId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (PrestamoCuotaDetalleRow c in cuotas)
                {
                    using var cmdDet = new SqlCommand(@"
INSERT INTO dbo.PrestamoDetalleCuotas
(PrestamoId, NumeroCuota, FechaVencimiento, Capital, Interes, MoraPotencial, Total)
VALUES
(@PrestamoId, @NumeroCuota, @FechaVencimiento, @Capital, @Interes, @MoraPotencial, @Total);", conn, tx);
                    cmdDet.Parameters.AddWithValue("@PrestamoId", prestamoId);
                    cmdDet.Parameters.AddWithValue("@NumeroCuota", c.NumeroCuota);
                    cmdDet.Parameters.AddWithValue("@FechaVencimiento", c.FechaVencimiento.Date);
                    cmdDet.Parameters.AddWithValue("@Capital", c.Capital);
                    cmdDet.Parameters.AddWithValue("@Interes", c.Interes);
                    cmdDet.Parameters.AddWithValue("@MoraPotencial", c.MoraPotencial);
                    cmdDet.Parameters.AddWithValue("@Total", c.Total);
                    cmdDet.ExecuteNonQuery();
                }

                tx.Commit();
                return prestamoId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    public sealed class PrestamoCuotaDetalleRow
    {
        public int NumeroCuota { get; init; }
        public DateTime FechaVencimiento { get; init; }
        public decimal Capital { get; init; }
        public decimal Interes { get; init; }
        public decimal MoraPotencial { get; init; }
        public decimal Total { get; init; }
    }
}
