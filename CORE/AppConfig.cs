using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace CORE
{
    public static class AppConfig
    {
        public static string ConnectionString =>
        @"Data Source=(localdb)\MSSQLLocalDB;
          Initial Catalog=MF CYBER DB;
          Integrated Security=True";
        public static bool ModoPrueba
        {
            get
            {
                string valor = ConfigurationManager.AppSettings["ModoPrueba"];

                return valor != null && valor.ToLower() == "true";
            }
        }
    }
}
