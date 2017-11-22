using ConCredito_Vendimia.App_Data;
using ConCredito_Vendimia.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;

namespace ConCredito_Vendimia.Controllers
{
    public class HomeController : Controller
    {
        public Clientes EntidadClientes { get; set; }
        public List<Clientes> ListaClientes { get; set; }
        public Articulos EntidadArticulos { get; set; }
        public List<Articulos> ListaArticulos { get; set; }
        public Configuraciones EntidadConfiguraciones { get; set; }
        public List<Configuraciones> ListaConfiguraciones { get; set; }
        public SqlConnection Conexion { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ObtenerClientesAutoComplete()
        {
            ConsultarClientes();

            if (ListaClientes == null)
            {
                ListaClientes = new List<Clientes>();
            }
            
            return Json(ListaClientes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerArticulosAutoComplete()
        {
            ConsultarArticulos();

            if (ListaArticulos == null)
            {
                ListaArticulos = new List<Articulos>();
            }

            return Json(ListaArticulos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerClientes([DataSourceRequest]DataSourceRequest request)
        {
            ConsultarClientes();

            if (ListaClientes == null)
            {
                ListaClientes = new List<Clientes>();
            }

            return Json(ListaClientes.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarClientes()
        {
            StringBuilder Sentencia = new StringBuilder();

            SqlCommand Comando = null;
            SqlDataReader Reader = null;

            Sentencia.AppendLine("SELECT ");
            Sentencia.AppendLine("	C.IDCLIENTE, ");
            Sentencia.AppendLine("	C.NOMBRE, ");
            Sentencia.AppendLine("	C.APELLIDOPATERNO, ");
            Sentencia.AppendLine("	C.APELLIDOMATERNO, ");
            Sentencia.AppendLine("	C.RFC, ");
            Sentencia.AppendLine("	C.FECHA, ");
            Sentencia.AppendLine("	C.ESTATUS ");
            Sentencia.AppendLine("FROM CLIENTES C ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);
                Reader = Comando.ExecuteReader();

                ListaClientes = new List<Clientes>();

                while (Reader.Read())
                {
                    EntidadClientes = new Clientes();
                    int Indice = 0;

                    EntidadClientes.IdCliente = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                    EntidadClientes.Nombre = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadClientes.ApellidoPaterno = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadClientes.ApellidoMaterno = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadClientes.RFC = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadClientes.Fecha = (Reader[Indice] != DBNull.Value ? Reader.GetDateTime(Indice) : DateTime.MinValue); Indice++;
                    EntidadClientes.Estatus = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;

                    ListaClientes.Add(EntidadClientes);
                }
            }
            catch (Exception ex)
            {
                ListaClientes = null;
                throw;
            }
            finally
            {
                if (Conexion != null)
                {
                    if (Conexion.State == ConnectionState.Open)
                    {
                        Conexion.Close();
                    }

                    Conexion.Dispose();
                    Conexion = null;

                    ConexionClass.CerrarConexion();
                }

                if (Comando != null)
                {
                    Comando.Dispose();
                    Comando = null;
                }

                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
        }

        public ActionResult ObtenerArticulos([DataSourceRequest]DataSourceRequest request)
        {
            ConsultarArticulos();

            if (ListaArticulos == null)
            {
                ListaArticulos = new List<Articulos>();
            }

            return Json(ListaArticulos.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarArticulos()
        {
            StringBuilder Sentencia = new StringBuilder();

            SqlCommand Comando = null;
            SqlDataReader Reader = null;

            Sentencia.AppendLine("SELECT ");
            Sentencia.AppendLine("  A.IDARTICULO, ");
            Sentencia.AppendLine("  A.DESCRIPCION, ");
            Sentencia.AppendLine("  A.EXISTENCIA, ");
            Sentencia.AppendLine("  A.PRECIO, ");
            Sentencia.AppendLine("  A.FECHA, ");
            Sentencia.AppendLine("  A.MODELO, ");
            Sentencia.AppendLine("  A.ESTATUS ");            
            Sentencia.AppendLine("FROM ARTICULOS A ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);
                Reader = Comando.ExecuteReader();

                ListaArticulos = new List<Articulos>();

                while (Reader.Read())
                {
                    EntidadArticulos = new Articulos();
                    int Indice = 0;

                    EntidadArticulos.IdArticulo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Descripcion = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Existencia = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadArticulos.Precio = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadArticulos.Fecha = (Reader[Indice] != DBNull.Value ? Reader.GetDateTime(Indice) : DateTime.MinValue); Indice++;
                    EntidadArticulos.Modelo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Estatus = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;

                    ListaArticulos.Add(EntidadArticulos);
                }
            }
            catch (Exception ex)
            {
                ListaArticulos = null;
                throw;
            }
            finally
            {
                if (Conexion != null)
                {
                    if (Conexion.State == ConnectionState.Open)
                    {
                        Conexion.Close();
                    }

                    Conexion.Dispose();
                    Conexion = null;

                    ConexionClass.CerrarConexion();
                }

                if (Comando != null)
                {
                    Comando.Dispose();
                    Comando = null;
                }

                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
        }

        public ActionResult ObtenerConfiguraciones([DataSourceRequest]DataSourceRequest request)
        {
            ConsultarConfiguraciones();

            if (ListaConfiguraciones == null)
            {
                ListaConfiguraciones = new List<Configuraciones>();
            }

            return Json(ListaConfiguraciones.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarConfiguraciones()
        {
            StringBuilder Sentencia = new StringBuilder();

            SqlCommand Comando = null;
            SqlDataReader Reader = null;

            Sentencia.AppendLine("SELECT ");
            Sentencia.AppendLine("  C.IDCONFIGURACION, ");
            Sentencia.AppendLine("  C.TASAFINANCIAMIENTO, ");
            Sentencia.AppendLine("  C.PORCENTAJENGANCHE, ");
            Sentencia.AppendLine("  C.PLAZOMAXIMO ");
            Sentencia.AppendLine("FROM CONFIGURACIONES C ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);
                Reader = Comando.ExecuteReader();
                
                while (Reader.Read())
                {
                    EntidadConfiguraciones = new Configuraciones();
                    int Indice = 0;

                    EntidadConfiguraciones.IdConfiguracion = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                    EntidadConfiguraciones.TasaFinanciamiento = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadConfiguraciones.PorcentajeEnganche = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadConfiguraciones.PlazoMaximo = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (Conexion != null)
                {
                    if (Conexion.State == ConnectionState.Open)
                    {
                        Conexion.Close();
                    }

                    Conexion.Dispose();
                    Conexion = null;

                    ConexionClass.CerrarConexion();
                }

                if (Comando != null)
                {
                    Comando.Dispose();
                    Comando = null;
                }

                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
        }
    }
}
