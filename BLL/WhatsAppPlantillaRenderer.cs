using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// Sustituye variables {CLAVE} y omite bloques vacíos.
    /// No altera montos ni estados: solo el texto del mensaje.
    /// </summary>
    public static class WhatsAppPlantillaRenderer
    {
        private static readonly Regex BloqueOpcional = new(
            @"\[\[(?<clave>[A-Za-z0-9_]+)\]\](?<cuerpo>.*?)\[\[/\k<clave>\]\]",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex Token = new(
            @"\{([A-Za-z0-9_]+)\}",
            RegexOptions.Compiled);

        private static readonly Regex EtiquetaVacia = new(
            @"^[^:\r\n]{1,40}:\s*$",
            RegexOptions.Compiled);

        public static string Aplicar(string? plantilla, IReadOnlyDictionary<string, string>? variables)
        {
            if (string.IsNullOrEmpty(plantilla))
                return string.Empty;

            var vars = variables ?? new Dictionary<string, string>();
            string texto = BloqueOpcional.Replace(plantilla, m =>
            {
                string clave = m.Groups["clave"].Value;
                if (!TryValor(vars, clave, out string valor) || string.IsNullOrWhiteSpace(valor))
                    return string.Empty;

                return m.Groups["cuerpo"].Value.Replace("{" + clave + "}", valor, StringComparison.OrdinalIgnoreCase);
            });

            texto = Token.Replace(texto, m =>
            {
                string clave = m.Groups[1].Value;
                return TryValor(vars, clave, out string valor) ? valor : m.Value;
            });

            var sb = new StringBuilder();
            bool anteriorVacia = false;
            foreach (string cruda in texto.Replace("\r\n", "\n").Split('\n'))
            {
                string linea = cruda.TrimEnd();
                if (Token.IsMatch(linea))
                    continue;
                if (EtiquetaVacia.IsMatch(linea.Trim()))
                    continue;

                bool vacia = string.IsNullOrWhiteSpace(linea);
                if (vacia)
                {
                    if (sb.Length == 0 || anteriorVacia)
                        continue;
                    anteriorVacia = true;
                    sb.AppendLine();
                    continue;
                }

                anteriorVacia = false;
                sb.AppendLine(linea.TrimEnd());
            }

            return sb.ToString().Trim();
        }

        private static bool TryValor(IReadOnlyDictionary<string, string> vars, string clave, out string valor)
        {
            foreach (var par in vars)
            {
                if (!string.Equals(par.Key, clave, StringComparison.OrdinalIgnoreCase))
                    continue;

                valor = par.Value ?? string.Empty;
                return true;
            }

            valor = string.Empty;
            return false;
        }
    }
}
