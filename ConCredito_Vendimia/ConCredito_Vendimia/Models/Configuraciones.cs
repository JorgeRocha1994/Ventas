using System;

namespace ConCredito_Vendimia.Models
{
    public class Configuraciones
    {
        public int IdConfiguracion { get; set; }
        public double TasaFinanciamiento { get; set; }
        public double PorcentajeEnganche { get; set; }
        public int PlazoMaximo { get; set; }
    }
}