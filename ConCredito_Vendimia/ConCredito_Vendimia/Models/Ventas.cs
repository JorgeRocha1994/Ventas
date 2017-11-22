using System;
using System.Collections.Generic;

namespace ConCredito_Vendimia.Models
{
    public class Ventas
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public double Enganche { get; set; }
        public double BonificacionEnganche { get; set; }
        public double TotalAdeudo { get; set; }
        public double TotalPagar { get; set; }
        public double PrecioContado { get; set; }
        public double ImporteAbono { get; set; }
        public double ImporteAhorro { get; set; }
        public int PlazoPago { get; set; }
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public List<VentasDetalle> ListaDetalles { get; set; }

        public string Cliente
        {
            get
            {
                return string.Concat(IdCliente.ToString("0000"), " - ", NombreCliente, " ", ApellidoPaterno, " ", ApellidoMaterno);
            }
        }
    }
}