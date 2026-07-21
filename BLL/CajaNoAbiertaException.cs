using System;

namespace BLL
{
    public class CajaNoAbiertaException : Exception
    {
        public CajaNoAbiertaException()
            : base("No hay caja abierta hoy.")
        {
        }
    }
}