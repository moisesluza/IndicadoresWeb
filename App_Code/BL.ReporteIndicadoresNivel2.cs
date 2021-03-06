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
            CalcularIndicadoresPorPrioridad(ref dtAgrupada, dtAgrupadaEstados);

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
            CalcularIndicadoresPorPrioridad(ref dtAgrupada, dtFiltrada);

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
            CalcularIndicadoresPorTipoSede(ref dtAgrupada, dtAgrupadaEstados);

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
            CalcularIndicadoresPorTipoSede(ref dtAgrupada, dtFiltrada);

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
            //dtFecFin = new DateTime(2012, 8, 1);

            lsEstado = new List<string>();
            lsEstado.Add("CL");
            lsEstado.Add("RE");

            lsGrupo = new List<string>();
            lsGrupo.Add("SOPORTE EN SITIO - LIMA");
            lsGrupo.Add("SOPORTE EN SITIO - PROV");

            try
            {
                dtNivel2 = objTpE.Listar_TiemposPorEstado(dtFecIni, dtFecFin, lsEstado, lsGrupo, new List<string>(), "I");
                dtNivel2 = DataHelper.Filter(dtNivel2, "Prioridad in ('0','1','2','3','4')");
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurri� un error con la Base de datos cuando se intent� obtener los tiempos por estado.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurri� un error no controlado cuando se intento obtener los tiempos por estado.", ex);
            }

        }
        
        private void AgregarTotales(ref DataTable i_dtOrigen)
        {
            i_dtOrigen.Columns.Add("SLA", typeof(int));
            i_dtOrigen.Columns.Add("Total_Tickets", typeof(int));
            i_dtOrigen.Columns.Add("Cumple_SLA", typeof(int));
            i_dtOrigen.Columns.Add("No_Cumple_SLA", typeof(int));
            i_dtOrigen.Columns.Add("Porcentaje", typeof(int));
            i_dtOrigen.Columns.Add("indSLACumplido", typeof(int));
        }

        private void CalcularIndicadoresPorPrioridad(ref DataTable i_dtAgrupada, DataTable i_dtDatos)
        {
            int iSla = 0;
            int iCantTktCumpleSLA = 0;
            int iCantTktNoCumpleSLA = 0;
            int iTotalTkt = 0;
            int iIndSLACumplido = 0;
            double dPorc = 0;

            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_RESPUESTA_SOLUCION"], out iSla);

            for (int i = 0; i < i_dtAgrupada.Rows.Count; i++)
            {
                iCantTktCumpleSLA = Convert.ToInt32(i_dtDatos.Compute("Count(tiempo)", "Prioridad" + " = '" + i_dtAgrupada.Rows[i]["Prioridad"] + "' and tiempo <= " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + ""));
                iCantTktNoCumpleSLA = Convert.ToInt32(i_dtDatos.Compute("Count(tiempo)", "Prioridad" + " = '" + i_dtAgrupada.Rows[i]["Prioridad"] + "' and tiempo > " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + ""));
                iTotalTkt = CalcularTotalTicketsPorPrioridadOP(i_dtAgrupada.Rows[i]["Prioridad"].ToString());
                dPorc = (Convert.ToDouble(iCantTktCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
                dPorc = Math.Round(dPorc);
                iIndSLACumplido = dPorc >= iSla ? 1 : 0;

                i_dtAgrupada.Rows[i]["SLA"] = iSla;
                i_dtAgrupada.Rows[i]["Total_Tickets"] = iTotalTkt;
                i_dtAgrupada.Rows[i]["Cumple_SLA"] = iCantTktCumpleSLA;
                i_dtAgrupada.Rows[i]["No_Cumple_SLA"] = iCantTktNoCumpleSLA;
                i_dtAgrupada.Rows[i]["Porcentaje"] = dPorc;
                i_dtAgrupada.Rows[i]["indSLACumplido"] = iIndSLACumplido;

            }
        }

        private void CalcularIndicadoresPorTipoSede(ref DataTable i_dtAgrupada, DataTable i_dtDatos)
        {
            int iSla = 0;
            int iCantTktCumpleSLA = 0;
            int iCantTktNoCumpleSLA = 0;
            int iTotalTkt = 0;
            int iIndSLACumplido = 0;
            double dPorc = 0;

            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_RESPUESTA_SOLUCION"], out iSla);

            for (int i = 0; i < i_dtAgrupada.Rows.Count; i++)
            {
                iCantTktCumpleSLA = Convert.ToInt32(i_dtDatos.Compute("Count(tiempo)", "Tipo_Sede_Usuario" + " = '" + i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"] + "' and tiempo <= " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + ""));
                iCantTktNoCumpleSLA = Convert.ToInt32(i_dtDatos.Compute("Count(tiempo)", "Tipo_Sede_Usuario" + " = '" + i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"] + "' and tiempo > " + i_dtAgrupada.Rows[i]["Tiempo_Minimo"] + ""));
                iTotalTkt = CalcularTotalTicketsPorTipoSedeODyOR(i_dtAgrupada.Rows[i]["Tipo_Sede_Usuario"].ToString());
                dPorc = (Convert.ToDouble(iCantTktCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
                dPorc = Math.Round(dPorc);
                iIndSLACumplido = dPorc >= iSla ? 1 : 0;

                i_dtAgrupada.Rows[i]["SLA"] = iSla;
                i_dtAgrupada.Rows[i]["Total_Tickets"] = iTotalTkt;
                i_dtAgrupada.Rows[i]["Cumple_SLA"] = iCantTktCumpleSLA;
                i_dtAgrupada.Rows[i]["No_Cumple_SLA"] = iCantTktNoCumpleSLA;
                i_dtAgrupada.Rows[i]["Porcentaje"] = dPorc;
                i_dtAgrupada.Rows[i]["indSLACumplido"] = iIndSLACumplido;
            }

        }

        private int CalcularTotalTicketsPorPrioridadOP(string i_sPrioridad)
        {
            //Se filtran las sedes
            DataTable dtFiltrada = DataHelper.Filter(dtNivel2, "Tipo_Sede_Usuario='OP' and Prioridad = '" + i_sPrioridad + "'");

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
    }

    
}


