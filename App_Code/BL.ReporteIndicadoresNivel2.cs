using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for BlTiemposPorEstado
    /// </summary>
    public class ReporteIndicadoresNivel2
    {
        private static ReporteIndicadoresNivel2 objRpt = null;
        private DataTable dtNivel2 = null;

        private ReporteIndicadoresNivel2()
        {
        }

        public static ReporteIndicadoresNivel2 getInstance()
        {
            if (objRpt == null)
            {
                objRpt = new ReporteIndicadoresNivel2();
            }

            return objRpt;
        }

        public DataTable ObtenerRptTiempoRespuestaOP()
        {
            //Se filtran las sedes
            DataTable dtFiltrada=DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario='OP' and Estado in ('Asignado','Registrado') and Prioridad <> 'Ninguno'");

            //Se agrupan los tiempos de los estados Asignado y en proceso
            DataTable dtAgrupadaEstados = DataHelper.Distinct(dtFiltrada, new String[] { "obj_id", "Prioridad", "Tiempo_Minimo" }, "obj_id");
            dtAgrupadaEstados.Columns.Add(new DataColumn("tiempo",typeof(int)));
            int iTiempoRpta = 0;
            foreach (DataRow dr in dtAgrupadaEstados.Rows)
            {
                DataRow[] drs = dtFiltrada.Select("obj_id=" + dr["obj_id"],"Estado");
                int.TryParse(drs[0]["tiempo"].ToString(), out iTiempoRpta);
                //Si el tiempo en estado Asignado (drs[0]) es 0 se utiliza el tiempo en estado registrado drs[1]
                if (drs.Length > 0 || iTiempoRpta > 0)
                    dr["tiempo"] = iTiempoRpta;
                else
                    dr["tiempo"] = int.Parse(drs[1]["tiempo"].ToString());
            }

            //Se obtienen los valores de prioridad agrupados
            DataTable dtAgrupada = DataHelper.Distinct(dtAgrupadaEstados, new String[] { "Prioridad", "Tiempo_Minimo" }, "Prioridad");

            //Se agregan las columnas de totales
            AgregarTotales(ref dtAgrupada);

            //Se calculan los totales
            CalcularTotalesTicketsPorPrioridad(ref dtAgrupada, dtAgrupadaEstados);

            dtAgrupada.TableName = "TiempoRespuestaOP";
            return dtAgrupada; 
        }

        public DataTable ObtenerRptTiempoSolucionOP()
        {
            //Se filtran las sedes
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario='OP' and Estado='En proceso' and Prioridad <> 'Ninguno'");

            //Se obtienen los valores de prioridad agrupados
            DataTable dtAgrupada = DataHelper.Distinct(dtFiltrada, new String[] { "Prioridad", "Tiempo_Minimo" }, "Prioridad");

            //Se agregan las columnas de totales
            AgregarTotales(ref dtAgrupada);

            //Se calculan los totales
            CalcularTotalesTicketsPorPrioridad(ref dtAgrupada, dtFiltrada);

            dtAgrupada.TableName = "TiempoSolucionOP";
            return dtAgrupada;
        }

        public DataTable ObtenerRptTiempoRespuestaODyOR()
        {
            //Se filtran las sedes
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario in ('OD','OR') and Estado in ('Asignado','Registrado') and Prioridad <> 'Ninguno'");
            
            //Se agrupan los tiempos de los estados Asignado y en proceso
            DataTable dtAgrupadaEstados = DataHelper.Distinct(dtFiltrada, new String[] { "obj_id", "Tipo_Sede_Usuario", "Tiempo_Minimo" }, "obj_id");
            dtAgrupadaEstados.Columns.Add(new DataColumn("tiempo", typeof(int)));
            int iTiempoRpta = 0;
            foreach (DataRow dr in dtAgrupadaEstados.Rows)
            {
                DataRow[] drs = dtFiltrada.Select("obj_id=" + dr["obj_id"], "Estado");
                int.TryParse(drs[0]["tiempo"].ToString(), out iTiempoRpta);
                //Si el tiempo en estado Asignado (drs[0]) es 0 se utiliza el tiempo en estado registrado drs[1]
                if (drs.Length > 0 || iTiempoRpta > 0)
                    dr["tiempo"] = iTiempoRpta;
                else
                    dr["tiempo"] = int.Parse(drs[1]["tiempo"].ToString());
            }

            //Se obtienen los valores de prioridad agrupados
            DataTable dtAgrupada = DataHelper.Distinct(dtAgrupadaEstados, new String[] { "Tipo_Sede_Usuario", "Tiempo_Minimo" }, "Tipo_Sede_Usuario");

            //Se agregan las columnas de totales
            AgregarTotales(ref dtAgrupada);

            //Se calculan los totales
            CalcularTotalesPorTipoSede(ref dtAgrupada, dtAgrupadaEstados);

            dtAgrupada.TableName = "TiempoRespuestaODyOR";
            return dtAgrupada;
        }

        public DataTable ObtenerRptTiempoSolucionODyOR()
        {
            //Se filtran las sedes
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario in ('OD','OR') and Estado='En proceso' and Prioridad <> 'Ninguno'");

            //Se obtienen los valores de prioridad agrupados
            DataTable dtAgrupada = DataHelper.Distinct(dtFiltrada, new String[] { "Tipo_Sede_Usuario", "Tiempo_Minimo" }, "Tipo_Sede_Usuario");

            //Se agregan las columnas de totales
            AgregarTotales(ref dtAgrupada);

            //Se calculan los totales
            CalcularTotalesPorTipoSede(ref dtAgrupada, dtFiltrada);

            dtAgrupada.TableName = "TiempoSolucionODyOR";
            return dtAgrupada;
        }

        public void obtenerTiemposPorEstado()
        {
            Ticket objTpE = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            List<String> lsGrupo = null;
            List<String> lsEstado = null;

            objTpE = new Ticket();
            dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtFecFin = DateTime.Now;

            //dtFecIni = new DateTime(2012, 7, 1);
            //dtFecFin = new DateTime(2012, 7, 30);

            lsEstado = new List<string>();
            lsEstado.Add("CL");
            lsEstado.Add("RE");

            lsGrupo = new List<string>();
            lsGrupo.Add("SOPORTE EN SITIO - LIMA");
            lsGrupo.Add("SOPORTE EN SITIO - PROV");

            try
            {
                dtNivel2 = objTpE.Listar_TiemposPorEstado(dtFecIni, dtFecFin, lsEstado, lsGrupo);
                //Comentado ya que no se ha informado si es que el error de los tickets mal registrados se solucionará 
                //en el reporte o en la información del service desk
                //CompletarTiempoSolucion();
                
                //dt = objTpE.Listar_TiemposPorEstado(new DateTime(DateTime.Today.Year, 2, 1), new DateTime(DateTime.Today.Year, 2, 29, 23, 59, 59), lsEstado, lsGrupo);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentó obtener los tiempos por estado.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intento obtener los tiempos por estado.", ex);
            }

        }
        
        private void AgregarTotales(ref DataTable i_dtOrigen)
        {
            i_dtOrigen.Columns.Add("SLA", typeof(int));
            i_dtOrigen.Columns.Add("Total_Tickets", typeof(int));
            i_dtOrigen.Columns.Add("Cumple_SLA", typeof(int));
            i_dtOrigen.Columns.Add("No_Cumple_SLA", typeof(int));
            i_dtOrigen.Columns.Add("Porcentaje", typeof(int));
        }

        private void CalcularTotalesTicketsPorPrioridad(ref DataTable i_dtAgrupada, DataTable i_dtDatos)
        {
            int iSla = 0;

            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_RESPUESTA_SOLUCION"], out iSla);
            for (int i = 0; i < i_dtAgrupada.Rows.Count; i++)
            {
                i_dtAgrupada.Rows[i]["SLA"] = iSla;
                i_dtAgrupada.Rows[i]["Total_Tickets"] = CalcularTotalTicketsPorPrioridadOP(Convert.ToInt32(i_dtAgrupada.Rows[i]["Prioridad"]));
                i_dtAgrupada.Rows[i]["Cumple_SLA"] = i_dtDatos.Compute("Count(tiempo)", "Prioridad" + " = '" + i_dtAgrupada.Rows[i]["Prioridad"] + "' and tiempo <= " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + "");
                i_dtAgrupada.Rows[i]["No_Cumple_SLA"] = i_dtDatos.Compute("Count(tiempo)", "Prioridad" + " = '" + i_dtAgrupada.Rows[i]["Prioridad"] + "' and tiempo > " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + "");
                i_dtAgrupada.Rows[i]["Porcentaje"] = (double.Parse(i_dtAgrupada.Rows[i]["Cumple_SLA"].ToString()) / double.Parse(i_dtAgrupada.Rows[i]["Total_Tickets"].ToString())) * 100;
            }
        }

        private void CalcularTotalesPorTipoSede(ref DataTable i_dtAgrupada, DataTable i_dtDatos)
        {
            int iSla = 0;
            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_RESPUESTA_SOLUCION"], out iSla);
            for (int i = 0; i < i_dtAgrupada.Rows.Count; i++)
            {
                i_dtAgrupada.Rows[i]["SLA"] = iSla;
                //i_dtAgrupada.Rows[i]["Total_Tickets"] = i_dtDatos.Compute("Count(tiempo)", "Tipo_Sede_Usuario" + " = '" + i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"] + "'");
                i_dtAgrupada.Rows[i]["Total_Tickets"] = CalcularTotalTicketsPorTipoSedeODyOR(i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"].ToString());
                i_dtAgrupada.Rows[i]["Cumple_SLA"] = i_dtDatos.Compute("Count(tiempo)", "Tipo_Sede_Usuario" + " = '" + i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"] + "' and tiempo <= " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + "");
                i_dtAgrupada.Rows[i]["No_Cumple_SLA"] = i_dtDatos.Compute("Count(tiempo)", "Tipo_Sede_Usuario" + " = '" + i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"] + "' and tiempo > " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + "");
                i_dtAgrupada.Rows[i]["Porcentaje"] = (double.Parse(i_dtAgrupada.Rows[i]["Cumple_SLA"].ToString()) / double.Parse(i_dtAgrupada.Rows[i]["Total_Tickets"].ToString())) * 100;
            }

        }

        private int CalcularTotalTicketsPorPrioridadOP(int i_iPriotidad)
        {
            //Se filtran las sedes
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario='OP' and Prioridad = '" + i_iPriotidad.ToString() + "'");

            //Se hace disticnt de los tickets
            DataTable dtTickets = DataHelper.Distinct(dtFiltrada, new String[] { "obj_id" }, "Prioridad");

            return dtTickets.Rows.Count;
        }

        private int CalcularTotalTicketsPorTipoSedeODyOR(string i_sTipoSede)
        {
            //Se filtra por tipo de sede
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario='" + i_sTipoSede + "'");

            //Se hace disticnt de los tickets
            DataTable dtTickets = DataHelper.Distinct(dtFiltrada, new String[] { "obj_id" }, "obj_id");

            return dtTickets.Rows.Count;
        }

        /// <summary>
        /// Se encarga de completar los tiempos En Proceso para los tickets mal registrados 
        /// (No debe existir tickets de nivel 2 sin tiempos En Proceso)
        /// </summary>
        private void CompletarTiempoSolucion()
        {
            DataTable dtTickets = DataHelper.Distinct(dtNivel2, new String[] { "obj_id" }, "Prioridad");
            foreach (DataRow dr in dtTickets.Rows)
            {
                //Si el ticket no tiene tiempo "En Proceso", se crea el registro con tiempo cero
                if (dtNivel2.Select("obj_id=" + dr["obj_id"] + " and Estado = 'En Proceso'").Length == 0)
                {
                    dtNivel2.ImportRow(dr);
                    dtNivel2.Rows[dtNivel2.Rows.Count - 1]["Estado"] = "En Proceso";
                    dtNivel2.Rows[dtNivel2.Rows.Count - 1]["tiempo"] = 0;
                }
            }
            dtNivel2.AcceptChanges();
        }
    }

    
}


