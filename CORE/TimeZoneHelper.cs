using System;

namespace CORE
{
    public static class TimeZoneHelper
    {
        /// <summary>
        /// Devuelve la hora actual en la República Dominicana si es posible.
        /// Intenta encontrar primero la zona horaria Windows y luego IANA;
        /// si falla, devuelve DateTime.Now local.
        /// </summary>
        public static DateTime NowDominicanRepublic()
        {
            // IDs probables: Windows "SA Western Standard Time", IANA "America/Santo_Domingo"
            string[] candidates = new[] { "SA Western Standard Time", "America/Santo_Domingo" };

            foreach (var id in candidates)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                }
                catch
                {
                    // ignorar y probar siguiente
                }
            }

            // Como último recurso, usar la hora local del equipo
            return DateTime.Now;
        }
    }
}
