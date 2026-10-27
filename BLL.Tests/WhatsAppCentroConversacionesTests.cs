using BLL;

namespace BLL.Tests
{
    public class WhatsAppCentroConversacionesTests
    {
        [Fact]
        public void Ventana_abierta_dentro_de_24_horas()
        {
            var ahora = new DateTime(2026, 10, 21, 12, 0, 0);
            Assert.True(WhatsAppVentanaAtencion.EstaAbierta(ahora.AddHours(-23), ahora));
        }

        [Fact]
        public void Ventana_cerrada_sin_entrada_o_despues_de_24_horas()
        {
            var ahora = new DateTime(2026, 10, 21, 12, 0, 0);
            Assert.False(WhatsAppVentanaAtencion.EstaAbierta(null, ahora));
            Assert.False(WhatsAppVentanaAtencion.EstaAbierta(ahora.AddHours(-25), ahora));
        }

        [Theory]
        [InlineData(true, "accepted", "ENVIADO")]
        [InlineData(true, "queued", "ENVIADO")]
        [InlineData(true, "sent", "ENVIADO")]
        [InlineData(true, "delivered", "ENTREGADO")]
        [InlineData(true, "read", "LEIDO")]
        [InlineData(true, "failed", "FALLIDO")]
        [InlineData(true, "undelivered", "FALLIDO")]
        [InlineData(false, "accepted", "FALLIDO")]
        [InlineData(true, null, "ENVIADO")]
        public void Estado_aceptado_no_es_entregado(bool exito, string? status, string esperado)
        {
            string estado = WhatsAppEstadoChat.DesdeRespuestaApi(exito, status);
            Assert.Equal(esperado, estado);
            if (status is "accepted" or "queued" or "sent" or null)
                Assert.False(WhatsAppEstadoChat.EsEntregaConfirmada(estado));
        }

        [Fact]
        public void Estado_no_degrada_leido()
        {
            Assert.False(WhatsAppEstadoChat.DebeActualizar("LEIDO", "ENTREGADO"));
            Assert.False(WhatsAppEstadoChat.DebeActualizar("LEIDO", "FALLIDO"));
            Assert.True(WhatsAppEstadoChat.DebeActualizar("ENVIADO", "ENTREGADO"));
            Assert.True(WhatsAppEstadoChat.DebeActualizar("ENVIADO", "FALLIDO"));
            Assert.False(WhatsAppEstadoChat.DebeActualizar("ENTREGADO", "ENVIADO"));
        }

        [Fact]
        public void Plantilla_omite_bloque_y_etiqueta_vacia()
        {
            const string plantilla =
                "Hola {CLIENTE}\nSaldo: {SALDO}\n[[DETALLE_DEUDA]]Detalle: {DETALLE_DEUDA}\n[[/DETALLE_DEUDA]]";

            string texto = WhatsAppPlantillaRenderer.Aplicar(plantilla, new Dictionary<string, string>
            {
                ["CLIENTE"] = "Ana",
                ["SALDO"] = "",
                ["DETALLE_DEUDA"] = ""
            });

            Assert.Equal("Hola Ana", texto);
            Assert.DoesNotContain("{", texto);
        }

        [Fact]
        public void Plantilla_conserva_bloque_con_dato()
        {
            const string plantilla = "Hola {CLIENTE}\n[[DETALLE]]{DETALLE}[[/DETALLE]]";
            string texto = WhatsAppPlantillaRenderer.Aplicar(plantilla, new Dictionary<string, string>
            {
                ["CLIENTE"] = "Ana",
                ["DETALLE"] = "Cuota 2"
            });

            Assert.Contains("Cuota 2", texto);
            Assert.DoesNotContain("[[", texto);
        }

        [Fact]
        public void Vista_previa_de_catalogo_no_deja_tokens()
        {
            foreach (var def in WhatsAppPlantillaCatalogo.Centro)
            {
                string texto = WhatsAppPlantillaRenderer.Aplicar(def.Predeterminada, WhatsAppPlantillaCatalogo.DatosEjemplo);
                Assert.False(string.IsNullOrWhiteSpace(texto));
                Assert.DoesNotContain("{", texto);
            }
        }

        [Fact]
        public void Sin_content_sid_no_permite_plantilla()
        {
            var resultado = WhatsAppPlantillaCompatibilidad.Evaluar(false, "Hola Ana");
            Assert.False(resultado.PuedeEnviar);
        }

        [Fact]
        public void Texto_con_variable_sin_valor_no_se_envia()
        {
            var resultado = WhatsAppPlantillaCompatibilidad.Evaluar(true, "Hola {CLIENTE}");
            Assert.False(resultado.PuedeEnviar);
        }

        [Fact]
        public void Los_seis_avisos_del_centro_se_pueden_activar_o_apagar()
        {
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("PAGO_MEMBRESIA"));
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("DEUDA_CREADA"));
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("PROGRAMACION_ACTIVADA"));
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("VENCIMIENTO_PROXIMO"));
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("MEMBRESIA_VENCIDA"));
            Assert.True(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("RECORDATORIO_VENCIMIENTO_DEUDA"));
            Assert.False(WhatsAppAutomatizacionPolitica.PuedeDesactivarse("FACTURA_MEMBRESIA"));
        }

        [Fact]
        public void Los_dos_plazos_de_configuracion_se_conservan()
        {
            IReadOnlyList<int> plazos = WhatsAppAutomatizacionPolitica.PlazosActivos(new[] { 10, 3, 0, 10 });
            Assert.Equal(new[] { 10, 3 }, plazos);
        }

        [Theory]
        [InlineData(8, 59, false)]
        [InlineData(9, 0, true)]
        [InlineData(15, 30, true)]
        public void Aviso_de_membresia_vencida_empieza_a_las_9(int hora, int minuto, bool esperado)
        {
            var ahora = new DateTime(2026, 10, 27, hora, minuto, 0);
            Assert.Equal(esperado, WhatsAppAutomatizacionPolitica.EsHorarioAvisoMembresiaVencida(ahora));
        }

        [Fact]
        public void Aviso_de_membresia_vencida_se_repite_cada_tres_dias()
        {
            var lunes = new DateTime(2026, 10, 26, 9, 5, 0);

            Assert.True(WhatsAppAutomatizacionPolitica.AvisoMembresiaVencidaAunVigente(
                lunes, new DateTime(2026, 10, 26, 18, 0, 0)));
            Assert.True(WhatsAppAutomatizacionPolitica.AvisoMembresiaVencidaAunVigente(
                lunes, new DateTime(2026, 10, 28, 9, 0, 0)));
            Assert.False(WhatsAppAutomatizacionPolitica.AvisoMembresiaVencidaAunVigente(
                lunes, new DateTime(2026, 10, 29, 9, 0, 0)));
        }
    }
}
