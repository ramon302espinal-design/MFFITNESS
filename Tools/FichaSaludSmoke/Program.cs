using BLL;
using DTO;

var bll = new ClienteBLL();

Console.WriteLine("1) EnsureSchema + Alta completa...");

var ficha = new ClienteFichaSaludDTO
{
    EmergenciaNombre = "Ana Auditoria",
    EmergenciaParentesco = "Hermana",
    EmergenciaTelefono = "8095551212",
    EmergenciaTelefonoAlt = "8095553434",
    NingunaEnfermedad = true,
    ObjSalud = true,
    ObjTonificar = true,
    ExperienciaNivel = "Menos6Meses",
    HorarioPreferido = "Variado",
    HorarioVariadoDetalle = "Lunes-Miercoles manana",
    FechaIngreso = DateTime.Today,
    TomaMedicamentos = false,
    TieneAlergias = true,
    AlergiasDescripcion = "Penicilina",
    TieneCirugias = true,
    CirugiasDescripcion = "Apendicectomia",
    CirugiasFecha = new DateTime(2020, 5, 10),
    LesionRodilla = true,
    LesionDescripcion = "Esguince leve 2024"
};

if (!ClienteFichaSaludValidator.Validar(ficha, out string err))
{
    Console.WriteLine("VALIDACION FALLO: " + err);
    return 1;
}

int id;
try
{
    id = bll.AgregarConFicha(
        "CLIENTE AUDITORIA FICHA",
        new DateTime(1995, 3, 15),
        "Calle Test 123",
        "8091112233",
        "Masculino",
        ficha);
}
catch (Exception ex)
{
    Console.WriteLine("ALTA FALLO: " + ex);
    return 2;
}

Console.WriteLine("ClienteId=" + id);

var loaded = bll.ObtenerFichaSalud(id);
if (loaded == null)
{
    Console.WriteLine("FALLO: ficha no encontrada tras guardar.");
    bll.Eliminar(id);
    return 3;
}

bool ok =
    loaded.EmergenciaNombre == "Ana Auditoria"
    && loaded.NingunaEnfermedad
    && loaded.ObjSalud
    && loaded.ObjTonificar
    && loaded.ExperienciaNivel == "Menos6Meses"
    && loaded.HorarioPreferido == "Variado"
    && loaded.HorarioVariadoDetalle == "Lunes-Miercoles manana"
    && loaded.TieneAlergias
    && loaded.AlergiasDescripcion == "Penicilina"
    && loaded.TieneCirugias
    && loaded.CirugiasDescripcion == "Apendicectomia"
    && loaded.CirugiasFecha?.Date == new DateTime(2020, 5, 10)
    && loaded.LesionRodilla
    && loaded.LesionDescripcion == "Esguince leve 2024";

Console.WriteLine(ok ? "2) Lectura OK (round-trip)" : "2) Lectura INCOMPLETE");
if (!ok)
{
    Console.WriteLine($"Exp={loaded.ExperienciaNivel} Hor={loaded.HorarioPreferido} Alg={loaded.AlergiasDescripcion}");
}

// update merge
loaded.HorarioPreferido = "Noche";
loaded.HorarioVariadoDetalle = null;
loaded.ObjPerderGrasa = true;
var dal = new DL.ClienteFichaSaludDAL();
dal.Guardar(loaded);
var again = bll.ObtenerFichaSalud(id);
bool mergeOk = again?.HorarioPreferido == "Noche" && again.ObjPerderGrasa;
Console.WriteLine(mergeOk ? "3) MERGE update OK" : "3) MERGE update FALLO");

bll.Eliminar(id);
var afterDel = bll.ObtenerFichaSalud(id);
Console.WriteLine(afterDel == null ? "4) Cascade delete OK" : "4) Cascade delete FALLO");

// columnas
dal.EnsureSchema();
Console.WriteLine("5) EnsureSchema idempotente OK");

Console.WriteLine(ok && mergeOk && afterDel == null ? "AUDITORIA FICHA: PASS" : "AUDITORIA FICHA: FAIL");
return ok && mergeOk && afterDel == null ? 0 : 10;
