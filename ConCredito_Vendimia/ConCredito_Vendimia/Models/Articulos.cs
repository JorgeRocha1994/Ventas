using System;

namespace ConCredito_Vendimia.Models
{
    public class Articulos
    {
        public string IdArticulo { get; set; }
        public string Descripcion { get; set; }
        public double Existencia { get; set; }
        public double Precio { get; set; }
        public DateTime Fecha { get; set; }
        public string Modelo { get; set; }
        public string Estatus { get; set; }

        public string Articulo
        {
            get
            {
                return string.Concat(IdArticulo, " - ", Descripcion);
            }
        }
    }
}