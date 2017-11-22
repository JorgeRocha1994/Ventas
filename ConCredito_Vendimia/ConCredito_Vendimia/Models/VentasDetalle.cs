using System;

namespace ConCredito_Vendimia.Models
{
    public class VentasDetalle
    {
        public int IdVenta { get; set; }
        public int IdDetalle { get; set; }
        public string IdArticulo { get; set; }
        public string Articulo { get; set; }
        public double Existencia { get; set; }
        public string Modelo { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double PrecioInteres { get; set; }
        public double Importe { get; set; }
        public DateTime Fecha { get; set; }
        public string Estatus { get; set; }

        public string DescripcionArticulo
        {
            get
            {
                return string.Concat(IdArticulo, " - ", Articulo);
            }
        }
    }
}