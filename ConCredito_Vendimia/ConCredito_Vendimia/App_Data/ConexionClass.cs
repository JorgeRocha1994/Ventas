using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using System;
using System.Data;

namespace ConCredito_Vendimia.App_Data
{
    public class ConexionClass
    {
        private static SqlConnection Conexion { get; set; }

        public static SqlConnection ObtenerConexion()
        {
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/MyWebSiteRoot");
            ConnectionStringSettings connString;

            try
            {
                if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
                {
                    connString = rootWebConfig.ConnectionStrings.ConnectionStrings["VentasConnectionStrings"];
                    if (connString != null)
                    {
                        Conexion = new SqlConnection(connString.ToString());
                        Conexion.Open();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Conexion;
        }

        public static void CerrarConexion()
        {
            if (Conexion != null)
            {
                if (Conexion.State == ConnectionState.Open)
                {
                    Conexion.Close();
                }

                Conexion.Dispose();
                Conexion = null;
            }
        }
    }
}