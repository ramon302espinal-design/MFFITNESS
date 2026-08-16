using BLL;

int clienteId = args.Length > 0 && int.TryParse(args[0], out int id) ? id : 1;
bool resumenDeudas = args.Length > 1
    && string.Equals(args[1], "resumen", StringComparison.OrdinalIgnoreCase);

if (resumenDeudas)
{
    var deudaBLL = new DeudaBLL();
    bool enviado = deudaBLL.EnviarResumenDeudasCliente(clienteId);
    Console.WriteLine($"RESUMEN_DEUDAS cliente {clienteId}: {(enviado ? "ENVIADO" : "FALLO")}");
    Console.WriteLine(deudaBLL.UltimoDetalleWhatsApp ?? "(sin detalle)");
    return;
}

var verificador = new WhatsAppVerificador();
string reporte = verificador.VerificarYEnviarPrueba(clienteId);
Console.WriteLine(reporte);
