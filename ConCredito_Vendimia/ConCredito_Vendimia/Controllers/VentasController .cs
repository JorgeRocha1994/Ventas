using ConCredito_Vendimia.Models;
using ConCredito_Vendimia.App_Data;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace ConCredito_Vendimia.Controllers
{
    public class VentasController : Controller
    {
        public Ventas EntidadVenta { get; set; }
        public List<Ventas> ListaVenta { get; set; }

        public Articulos EntidadArticulos { get; set; }
        public List<Articulos> ListaArticulos { get; set; }

        public VentasDetalle EntidadVentasDetalle { get; set; }
        public List<VentasDetalle> ListaVentasDetalle { get; set; }

        public Configuraciones EntidadConfiguraciones { get; set; }

        public VentasController()
        {
            ConsultarConfiguraciones();

            ConsultarConfiguraciones();

            if (EntidadConfiguraciones != null)
            {
                TempData["EntidadConfiguraciones"] = EntidadConfiguraciones;
                TempData.Keep("EntidadConfiguraciones");
            }
        }

        public SqlConnection Conexion { get; set; }

        public ActionResult RegistroVentas()
        {
            return View("RegistroVentas");
        }

        public ActionResult ObtenerVentaConsecutiva()
        {
            JObject resultado = new JObject();
            try
            {
                int IdVenta = ConsultarVentaUltimo();

                resultado = new JObject(
                    new JProperty("Estatus", "Correcto"),
                    new JProperty("Resultado", IdVenta)
                    );
            }
            catch (Exception ex)
            {
                resultado = new JObject(
                    new JProperty("Estatus", "Error"),
                    new JProperty("Resultado", "No fue posible obtener el Consecutivo para la Venta.")
                    );
            }

            return Json(resultado.ToString(), JsonRequestBehavior.AllowGet);
        }

        private int ConsultarVentaUltimo()
        {
            int IdVenta = 0;
            StringBuilder Sentencia = new StringBuilder();

            SqlCommand Comando = null;
            SqlDataReader Reader = null;

            Sentencia.AppendLine("EXEC SP_ULTIMO_VENTAS @IDVENTA = 0 ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);

                Reader = Comando.ExecuteReader();

                while (Reader.Read())
                {
                    IdVenta = (Reader[0] != DBNull.Value ? Reader.GetInt32(0) : 0);
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

            return IdVenta;
        }

        public ActionResult ObtenerVentas([DataSourceRequest]DataSourceRequest request, string Busqueda, int Filtro, DateTime FechaInicial, DateTime FechaFinal)
        {
            ConsultarVentas(Busqueda, Filtro, FechaInicial, FechaFinal);

            if (ListaVenta == null)
            {
                ListaVenta = new List<Ventas>();
            }

            return Json(ListaVenta.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarVentas(string Busqueda, int Filtro, DateTime FechaInicial, DateTime FechaFinal)
        {
            StringBuilder Sentencia = new StringBuilder();
            
            SqlCommand Comando = null;
            SqlDataReader Reader = null;

            Sentencia.AppendLine("SELECT ");
            Sentencia.AppendLine("	V.IDVENTA, ");
            Sentencia.AppendLine("	V.ENGANCHE, ");
            Sentencia.AppendLine("	V.BONIFICACIONENGANCHE, ");
            Sentencia.AppendLine("	V.TOTALADEUDO, ");
            Sentencia.AppendLine("	V.TOTALPAGAR, ");
            Sentencia.AppendLine("	V.PRECIOCONTADO, ");
            Sentencia.AppendLine("	V.IMPORTEABONO, ");
            Sentencia.AppendLine("	V.IMPORTEAHORRO, ");
            Sentencia.AppendLine("	V.PLAZOPAGO, ");
            Sentencia.AppendLine("	V.FECHA, ");
            Sentencia.AppendLine("	V.ESTATUS, ");
            Sentencia.AppendLine("	C.IDCLIENTE, ");
            Sentencia.AppendLine("	C.NOMBRE, ");
            Sentencia.AppendLine("	C.APELLIDOPATERNO, ");
            Sentencia.AppendLine("	C.APELLIDOMATERNO ");
            Sentencia.AppendLine("FROM VENTAS V ");
            Sentencia.AppendLine("LEFT JOIN CLIENTES C ON ");
            Sentencia.AppendLine("	C.IDCLIENTE = V.IDCLIENTE ");
            Sentencia.AppendLine("WHERE ");
            Sentencia.AppendLine("	V.ESTATUS = 'Activo' ");
            Sentencia.AppendLine("	AND V.FECHA BETWEEN @FECHAINICIAL AND @FECHAFINAL ");

            if (!string.IsNullOrEmpty(Busqueda))
            {
                if (Filtro == 0) //Filtro por Ventas
                {
                    int IdVenta = 0;
                    int.TryParse(Busqueda, out IdVenta);

                    if(IdVenta > 0)
                    {
                        Sentencia.AppendLine("	AND V.IDVENTA = @BUSQUEDA ");
                    }
                }
                else //Filtro por Clientes
                {
                    int IdCliente = 0;
                    int.TryParse(Busqueda, out IdCliente);

                    if (IdCliente > 0)
                    {
                        Sentencia.AppendLine("	AND C.IDCLIENTE = @BUSQUEDA ");
                    }
                    else
                    {
                        Sentencia.AppendLine("	AND (C.NOMBRE LIKE @BUSQUEDA ");
                        Sentencia.AppendLine("	OR C.APELLIDOPATERNO LIKE @BUSQUEDA ");
                        Sentencia.AppendLine("  OR C.APELLIDOMATERNO LIKE @BUSQUEDA) ");
                    }
                }
            }

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);

                if (!string.IsNullOrEmpty(Busqueda))
                {
                    if (Filtro == 0) //Filtro por Ventas
                    {
                        int IdVenta = 0;
                        int.TryParse(Busqueda, out IdVenta);

                        if (IdVenta > 0)
                        {
                            Comando.Parameters.Add("@BUSQUEDA", SqlDbType.Int);
                            Comando.Parameters["@BUSQUEDA"].Value = Busqueda;
                        }
                    }
                    else //Filtro por Clientes
                    {
                        int IdCliente = 0;
                        int.TryParse(Busqueda, out IdCliente);

                        if (IdCliente > 0)
                        {
                            Comando.Parameters.Add("@BUSQUEDA", SqlDbType.Int);
                            Comando.Parameters["@BUSQUEDA"].Value = Busqueda;
                        }
                        else
                        {
                            Comando.Parameters.Add("@BUSQUEDA", SqlDbType.VarChar);
                            Comando.Parameters["@BUSQUEDA"].Value = "%" + Busqueda + "%";
                        }
                    }
                }

                Comando.Parameters.Add("@FECHAINICIAL", SqlDbType.Date);
                Comando.Parameters["@FECHAINICIAL"].Value = FechaInicial;
                Comando.Parameters.Add("@FECHAFINAL", SqlDbType.Date);
                Comando.Parameters["@FECHAFINAL"].Value = FechaFinal;

                Reader = Comando.ExecuteReader();

                ListaVenta = new List<Ventas>();

                while (Reader.Read())
                {
                    EntidadVenta = new Ventas();
                    int Indice = 0;

                    EntidadVenta.IdVenta = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                    EntidadVenta.Enganche = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.BonificacionEnganche = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.TotalAdeudo = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.TotalPagar = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.PrecioContado = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.ImporteAbono = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.ImporteAhorro = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVenta.PlazoPago = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                    EntidadVenta.Fecha = (Reader[Indice] != DBNull.Value ? Reader.GetDateTime(Indice) : DateTime.MinValue); Indice++;
                    EntidadVenta.Estatus = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVenta.IdCliente = (Reader[Indice] != DBNull.Value ? Reader.GetInt32(Indice) : 0); Indice++;
                    EntidadVenta.NombreCliente = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVenta.ApellidoPaterno = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVenta.ApellidoMaterno = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;

                    ListaVenta.Add(EntidadVenta);
                }
            }
            catch (Exception ex)
            {
                ListaVenta = null;
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

        public ActionResult ObtenerArticulo([DataSourceRequest]DataSourceRequest request, string IdArticulo, string ListaActual)
        {
            if(string.IsNullOrEmpty(IdArticulo) && string.IsNullOrEmpty(ListaActual))
            {
                return null;
            }

            ConsultarArticulo(IdArticulo);
            List<Articulos> ListaActualGrid = JsonConvert.DeserializeObject<List<Articulos>>(ListaActual);

            if(ListaActualGrid == null)
            {
                ListaActualGrid = new List<Articulos>();
            }

            ListaActualGrid.RemoveAll(item => item == null);
            ListaActualGrid.Add(EntidadArticulos);

            return Json(ListaActualGrid.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidarExistencia(string IdArticulo)
        {
            JObject resultado = new JObject();

            if (string.IsNullOrEmpty(IdArticulo))
            {
                resultado = new JObject(
                    new JProperty("Estatus", "Error"),
                    new JProperty("Resultado", "No fue posible obtener la clave del Artículo.")
                    );
            }
            else
            {
                ConsultarArticulo(IdArticulo);

                if (EntidadArticulos == null)
                {
                    resultado = new JObject(
                        new JProperty("Estatus", "Error"),
                        new JProperty("Resultado", "No fue posible obtener los datos del Artículo, asegúrese de que el articulo este activo o que no este cancelado.")
                        );
                }
                else
                {
                    resultado = new JObject(
                        new JProperty("Estatus", "Correcto"),
                        new JProperty("Resultado", EntidadArticulos.Existencia)
                        );
                }
            }

            return Json(resultado.ToString(), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarArticulo(string IdArticulo)
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
            Sentencia.AppendLine("  A.ESTATUS, ");
            Sentencia.AppendLine("  A.MODELO ");
            Sentencia.AppendLine("FROM ARTICULOS A ");
            Sentencia.AppendLine("WHERE ");
            Sentencia.AppendLine("  A.ESTATUS = 'Activo' ");
            Sentencia.AppendLine("  AND A.EXISTENCIA > 0 ");
            Sentencia.AppendLine("  AND A.IDARTICULO = @IDARTICULO ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);

                Comando.Parameters.Add("@IDARTICULO", SqlDbType.VarChar);
                Comando.Parameters["@IDARTICULO"].Value = IdArticulo;

                Reader = Comando.ExecuteReader();
                EntidadArticulos = new Articulos();

                while (Reader.Read())
                {                    
                    int Indice = 0;
                    EntidadArticulos.IdArticulo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Descripcion = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Existencia = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadArticulos.Precio = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadArticulos.Fecha = (Reader[Indice] != DBNull.Value ? Reader.GetDateTime(Indice) : DateTime.MinValue); Indice++;
                    EntidadArticulos.Estatus = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadArticulos.Modelo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
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

        public ActionResult AgregarArticulosVenta([DataSourceRequest]DataSourceRequest request, string IdArticulo, string ListaActual)
        {
            if (string.IsNullOrEmpty(IdArticulo) && string.IsNullOrEmpty(ListaActual))
            {
                return null;
            }

            List<VentasDetalle> ListaActualGrid = JsonConvert.DeserializeObject<List<VentasDetalle>>(ListaActual);

            if (ListaActualGrid == null)
            {
                ListaActualGrid = new List<VentasDetalle>();
            }

            ConsultarArticulosVenta(IdArticulo, ListaActualGrid);

            if (EntidadVentasDetalle == null)
            {
                EntidadVentasDetalle = new VentasDetalle();
            }

            ListaActualGrid.RemoveAll(item => item == null);
            ListaActualGrid.Add(EntidadVentasDetalle);

            return Json(ListaActualGrid.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private void ConsultarArticulosVenta(string IdArticulo, List<VentasDetalle> ListaActualGrid)
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
            Sentencia.AppendLine("  A.ESTATUS, ");
            Sentencia.AppendLine("  A.MODELO ");
            Sentencia.AppendLine("FROM ARTICULOS A ");
            Sentencia.AppendLine("WHERE ");
            Sentencia.AppendLine("  A.ESTATUS = 'Activo' ");
            Sentencia.AppendLine("  AND A.EXISTENCIA > 0 ");
            Sentencia.AppendLine("  AND A.IDARTICULO = @IDARTICULO ");

            try
            {
                Conexion = ConexionClass.ObtenerConexion();
                Comando = new SqlCommand(Sentencia.ToString(), Conexion);

                Comando.Parameters.Add("@IDARTICULO", SqlDbType.VarChar);
                Comando.Parameters["@IDARTICULO"].Value = IdArticulo;

                Reader = Comando.ExecuteReader();

                while (Reader.Read())
                {
                    int Indice = 0;
                    EntidadVentasDetalle = new VentasDetalle();

                    EntidadVentasDetalle.IdArticulo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVentasDetalle.Articulo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVentasDetalle.Existencia = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVentasDetalle.Precio = (Reader[Indice] != DBNull.Value ? Reader.GetDouble(Indice) : 0); Indice++;
                    EntidadVentasDetalle.Fecha = (Reader[Indice] != DBNull.Value ? Reader.GetDateTime(Indice) : DateTime.MinValue); Indice++;
                    EntidadVentasDetalle.Estatus = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                    EntidadVentasDetalle.Modelo = (Reader[Indice] != DBNull.Value ? Reader.GetString(Indice) : string.Empty); Indice++;
                }
            }
            catch (Exception ex)
            {
                EntidadVentasDetalle = null;
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

                #region Inicialización de campos para la Venta
                if (EntidadVentasDetalle != null)
                {
                    int IdVenta = ConsultarVentaUltimo();
                    int IdDetalle = 0;

                    if (IdVenta <= 0)
                    {
                        IdVenta = 1;
                    }
                    else
                    {
                        IdVenta++;
                    }

                    if (ListaActualGrid != null && ListaActualGrid.Count > 0)
                    {
                        IdDetalle = ListaActualGrid.Select(s => s.IdDetalle).Max();
                        IdDetalle++;
                    }
                    else
                    {
                        IdDetalle = 1;
                    }

                    EntidadVentasDetalle.IdVenta = IdVenta;
                    EntidadVentasDetalle.IdDetalle = IdDetalle;
                    EntidadVentasDetalle.Cantidad = 1;

                    if (TempData["EntidadConfiguraciones"] != null && !string.IsNullOrEmpty(TempData["EntidadConfiguraciones"].ToString()))
                    {
                        EntidadConfiguraciones = (Configuraciones)TempData["EntidadConfiguraciones"];
                        TempData.Keep("EntidadConfiguraciones");
                    }
                    else
                    {
                        if (EntidadConfiguraciones == null)
                        {
                            ConsultarConfiguraciones();
                        }                        
                    }

                    double PrecioInteres = EntidadVentasDetalle.Precio * (1 + (EntidadConfiguraciones.TasaFinanciamiento * EntidadConfiguraciones.PlazoMaximo) / 100);
                    EntidadVentasDetalle.PrecioInteres = PrecioInteres;

                    double Importe = EntidadVentasDetalle.PrecioInteres * EntidadVentasDetalle.Cantidad;
                    EntidadVentasDetalle.Importe = Importe;
                }
                #endregion
            }
        }

        public ActionResult ObtenerEntidadConfiguraciones()
        {
            ConsultarConfiguraciones();

            if (EntidadConfiguraciones == null)
            {
                EntidadConfiguraciones = new Configuraciones();
            }

            return Json(EntidadConfiguraciones, JsonRequestBehavior.AllowGet);
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

        public ActionResult Guardar(string entidadSerializada)
        {
            bool InsertoVenta = false;
            bool InsertoVentaDetalle = false;
            bool ModificoExistencia = false;

            StringBuilder Sentencia = new StringBuilder();
            JObject resultado = new JObject();

            SqlCommand Comando = null;
            SqlTransaction Transaction = null;

            if (!string.IsNullOrEmpty(entidadSerializada))
            {
                EntidadVenta = JsonConvert.DeserializeObject<Ventas>(entidadSerializada);

                int IdVentaConsecutivo = ConsultarVentaUltimo();
                EntidadVenta.IdVenta = IdVentaConsecutivo;
                EntidadVenta.Fecha = DateTime.Now;
                EntidadVenta.Estatus = "Activo";

                ListaVentasDetalle = EntidadVenta.ListaDetalles;
                ListaVentasDetalle.ForEach(f => f.IdVenta = EntidadVenta.IdVenta);

                Sentencia.AppendLine("INSERT INTO VENTAS ");
                Sentencia.AppendLine("	(IDVENTA, ");
                Sentencia.AppendLine("	IDCLIENTE, ");
                Sentencia.AppendLine("	ENGANCHE, ");
                Sentencia.AppendLine("	BONIFICACIONENGANCHE, ");
                Sentencia.AppendLine("	TOTALADEUDO, ");
                Sentencia.AppendLine("	TOTALPAGAR, ");
                Sentencia.AppendLine("	PRECIOCONTADO, ");
                Sentencia.AppendLine("	IMPORTEABONO, ");
                Sentencia.AppendLine("	IMPORTEAHORRO, ");
                Sentencia.AppendLine("	PLAZOPAGO, ");
                Sentencia.AppendLine("	FECHA, ");
                Sentencia.AppendLine("	ESTATUS) ");
                Sentencia.AppendLine("VALUES ");
                Sentencia.AppendLine("	(@IDVENTA, ");
                Sentencia.AppendLine("	@IDCLIENTE, ");
                Sentencia.AppendLine("	@ENGANCHE, ");
                Sentencia.AppendLine("	@BONIFICACIONENGANCHE, ");
                Sentencia.AppendLine("	@TOTALADEUDO, ");
                Sentencia.AppendLine("	@TOTALPAGAR, ");
                Sentencia.AppendLine("	@PRECIOCONTADO, ");
                Sentencia.AppendLine("	@IMPORTEABONO, ");
                Sentencia.AppendLine("	@IMPORTEAHORRO, ");
                Sentencia.AppendLine("	@PLAZOPAGO, ");
                Sentencia.AppendLine("	@FECHA, ");
                Sentencia.AppendLine("	@ESTATUS) ");

                try
                {
                    Conexion = ConexionClass.ObtenerConexion();
                    Transaction = Conexion.BeginTransaction();

                    Comando = new SqlCommand(Sentencia.ToString(), Conexion, Transaction);
                    
                    #region Parametros
                    Comando.Parameters.Add("@IDVENTA", SqlDbType.Int);
                    Comando.Parameters["@IDVENTA"].Value = EntidadVenta.IdVenta;

                    Comando.Parameters.Add("@IDCLIENTE", SqlDbType.Int);
                    Comando.Parameters["@IDCLIENTE"].Value = EntidadVenta.IdCliente;

                    Comando.Parameters.Add("@ENGANCHE", SqlDbType.Float);
                    Comando.Parameters["@ENGANCHE"].Value = EntidadVenta.Enganche;

                    Comando.Parameters.Add("@BONIFICACIONENGANCHE", SqlDbType.Float);
                    Comando.Parameters["@BONIFICACIONENGANCHE"].Value = EntidadVenta.BonificacionEnganche;

                    Comando.Parameters.Add("@TOTALADEUDO", SqlDbType.Float);
                    Comando.Parameters["@TOTALADEUDO"].Value = EntidadVenta.TotalAdeudo;

                    Comando.Parameters.Add("@TOTALPAGAR", SqlDbType.Float);
                    Comando.Parameters["@TOTALPAGAR"].Value = EntidadVenta.TotalPagar;

                    Comando.Parameters.Add("@PRECIOCONTADO", SqlDbType.Float);
                    Comando.Parameters["@PRECIOCONTADO"].Value = EntidadVenta.PrecioContado;

                    Comando.Parameters.Add("@IMPORTEABONO", SqlDbType.Float);
                    Comando.Parameters["@IMPORTEABONO"].Value = EntidadVenta.ImporteAbono;

                    Comando.Parameters.Add("@IMPORTEAHORRO", SqlDbType.Float);
                    Comando.Parameters["@IMPORTEAHORRO"].Value = EntidadVenta.ImporteAhorro;

                    Comando.Parameters.Add("@PLAZOPAGO", SqlDbType.Int);
                    Comando.Parameters["@PLAZOPAGO"].Value = EntidadVenta.PlazoPago;

                    Comando.Parameters.Add("@FECHA", SqlDbType.DateTime);
                    Comando.Parameters["@FECHA"].Value = EntidadVenta.Fecha;

                    Comando.Parameters.Add("@ESTATUS", SqlDbType.VarChar);
                    Comando.Parameters["@ESTATUS"].Value = EntidadVenta.Estatus;
                    #endregion

                    InsertoVenta = (Comando.ExecuteNonQuery() > 0);

                    if (InsertoVenta)
                    {
                        InsertoVentaDetalle = GuardarVentaDetalle(ListaVentasDetalle, Conexion, Transaction);

                        if(InsertoVentaDetalle)
                        {
                            ModificoExistencia = DisminuirExistencia(ListaVentasDetalle, Conexion, Transaction);

                            if (ModificoExistencia)
                            {
                                Transaction.Commit();
                                resultado = new JObject(
                                    new JProperty("Estatus", "Correcto"),
                                    new JProperty("Mensaje", "Bien Hecho, Tu venta ha sido registrada correctamente con el Folio: " + IdVentaConsecutivo.ToString("0000"))
                                    );
                            }
                            else
                            {
                                Transaction.Rollback();
                                resultado = new JObject(
                                    new JProperty("Estatus", "Error"),
                                    new JProperty("Mensaje", "No fue posible Modificar la Existencia de los Artículos el Detalle.")
                                    );
                            }
                        }
                        else
                        {
                            Transaction.Rollback();
                            resultado = new JObject(
                                new JProperty("Estatus", "Error"),
                                new JProperty("Mensaje", "No fue posible Guardar el Detalle de la Venta verifique los campos, intente de nuevo o mas tarde.")
                                );
                        }
                    }
                    else
                    {
                        Transaction.Rollback();
                        resultado = new JObject(
                            new JProperty("Estatus", "Error"),
                            new JProperty("Mensaje", "No fue posible Guardar la Venta verifique los campos, intente de nuevo o mas tarde.")
                            );
                    }
                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    resultado = new JObject(
                        new JProperty("Estatus", "Error"),
                        new JProperty("Mensaje", "No fue posible Guardar la Venta verifique los campos, intente de nuevo o mas tarde.")
                        );
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

                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }
                }
            }
            else
            {
                resultado = new JObject(
                        new JProperty("Estatus", "Error"),
                        new JProperty("Mensaje", "No fue posible Guardar la Venta verifique los campos, intente de nuevo o mas tarde.")
                        );
            }

            return Json(resultado.ToString(), JsonRequestBehavior.AllowGet);
        }

        private bool GuardarVentaDetalle(List<VentasDetalle> ListaVentasDetalle, SqlConnection Conexion, SqlTransaction Transaction)
        {
            bool InsertoVentaDetalle = false;
            int NumeroDeDetalles = 0;

            StringBuilder Sentencia = new StringBuilder();
            SqlCommand Comando = null;

            Sentencia.AppendLine("INSERT INTO VENTASDETALLE ");
            Sentencia.AppendLine("	(IDVENTA, ");
            Sentencia.AppendLine("	IDDETALLE, ");
            Sentencia.AppendLine("	IDARTICULO, ");
            Sentencia.AppendLine("	CANTIDAD, ");
            Sentencia.AppendLine("	PRECIO, ");
            Sentencia.AppendLine("	IMPORTE, ");
            Sentencia.AppendLine("	ESTATUS) ");
            Sentencia.AppendLine("VALUES ");
            Sentencia.AppendLine("	(@IDVENTA, ");
            Sentencia.AppendLine("	@IDDETALLE, ");
            Sentencia.AppendLine("	@IDARTICULO, ");
            Sentencia.AppendLine("	@CANTIDAD, ");
            Sentencia.AppendLine("	@PRECIO, ");
            Sentencia.AppendLine("	@IMPORTE, ");
            Sentencia.AppendLine("	@ESTATUS) ");

            try
            {
                foreach (VentasDetalle EntidadVenta in ListaVentasDetalle)
                {
                    Comando = new SqlCommand(Sentencia.ToString(), Conexion, Transaction);

                    EntidadVenta.Estatus = "Activo";

                    #region Parametros
                    Comando.Parameters.Add("@IDVENTA", SqlDbType.Int);
                    Comando.Parameters["@IDVENTA"].Value = EntidadVenta.IdVenta;

                    Comando.Parameters.Add("@IDDETALLE", SqlDbType.Int);
                    Comando.Parameters["@IDDETALLE"].Value = EntidadVenta.IdDetalle;

                    Comando.Parameters.Add("@IDARTICULO", SqlDbType.VarChar);
                    Comando.Parameters["@IDARTICULO"].Value = EntidadVenta.IdArticulo;

                    Comando.Parameters.Add("@CANTIDAD", SqlDbType.Float);
                    Comando.Parameters["@CANTIDAD"].Value = EntidadVenta.Cantidad;

                    Comando.Parameters.Add("@PRECIO", SqlDbType.Float);
                    Comando.Parameters["@PRECIO"].Value = EntidadVenta.PrecioInteres;

                    Comando.Parameters.Add("@IMPORTE", SqlDbType.Float);
                    Comando.Parameters["@IMPORTE"].Value = EntidadVenta.Importe;

                    Comando.Parameters.Add("@ESTATUS", SqlDbType.VarChar);
                    Comando.Parameters["@ESTATUS"].Value = EntidadVenta.Estatus;
                    #endregion

                    NumeroDeDetalles += Comando.ExecuteNonQuery();
                }

                InsertoVentaDetalle = (NumeroDeDetalles > 0);
            }
            catch (Exception ex)
            {
                return InsertoVentaDetalle;
            }

            return InsertoVentaDetalle;
        }

        private bool DisminuirExistencia(List<VentasDetalle> ListaVentasDetalle, SqlConnection Conexion, SqlTransaction Transaction)
        {
            bool ModificoExistencia = false;
            int NumeroDeDetalles = 0;

            StringBuilder Sentencia = new StringBuilder();
            SqlCommand Comando = null;

            Sentencia.AppendLine("EXEC SP_DISMINUIR_EXISTENCIA_ARTICULO @IDARTICULO, @EXISTENCIA");

            try
            {                
                foreach (VentasDetalle EntidadVenta in ListaVentasDetalle)
                {
                    Comando = new SqlCommand(Sentencia.ToString(), Conexion, Transaction);

                    Comando.Parameters.Add("@IDARTICULO", SqlDbType.VarChar);
                    Comando.Parameters["@IDARTICULO"].Value = EntidadVenta.IdArticulo;

                    Comando.Parameters.Add("@EXISTENCIA", SqlDbType.Float);
                    Comando.Parameters["@EXISTENCIA"].Value = EntidadVenta.Cantidad;

                    NumeroDeDetalles += Comando.ExecuteNonQuery();
                }

                ModificoExistencia = (NumeroDeDetalles > 0);
            }
            catch (Exception ex)
            {
                return ModificoExistencia;
            }

            return ModificoExistencia;
        }
    }
}
