using BLL;

int clienteId = args.Length > 0 && int.TryParse(args[0], out int id) ? id : 1;

var verificador = new WhatsAppVerificador();
string reporte = verificador.VerificarYEnviarPrueba(clienteId);
Console.WriteLine(reporte);
