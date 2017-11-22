using System;

namespace ConCredito_Vendimia.Models
{
    public class Clientes
    {
        public int IdCliente{ get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string RFC { get; set; }
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; }


        public string NombreCompleto
        {
            get
            {
                return string.Concat(Nombre, " ", ApellidoPaterno, " ", ApellidoMaterno);
            }
        }

        public string Cliente
        {
            get
            {
                return string.Concat(IdCliente.ToString("0000"), " - ", NombreCompleto);
            }
        }
    }
}