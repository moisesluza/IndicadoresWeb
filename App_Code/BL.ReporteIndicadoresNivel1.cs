using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Data.SqlClient;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for ReporteIndicadoresNivel1
    /// </summary>
    public class ReporteIndicadoresNivel1
    {
        private static ReporteIndicadoresNivel1 objRpt = null;
        private DataTable dtDatos = null;

        private ReporteIndicadoresNivel1()
        {
        }

        public static ReporteIndicadoresNivel1 getInstance()
        {
            if (objRpt == null)
                objRpt = new ReporteIndicadoresNivel1();
            return objRpt;
        }

        public DataTable ObtenerRtpIndicadoresNivel1()
        {
            dtDatos = obtenerTicketsNivel1();

            DataTable dtRep = GenerarTablaRpt();

            CalcularTotales(ref dtRep, dtDatos);
            
            return dtRep;
        }

        private DataTable obtenerTicketsNivel1()
        {
            Ticket objTkt = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            List<String> lsGrupo = null;
            List<String> lsEstado = null;

            objTkt = new Ticket();
            dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtFecFin = DateTime.Now;

            lsEstado = new List<string>();
            lsEstado.Add("CL");
            lsEstado.Add("RE");

            lsGrupo = new List<string>();
            lsGrupo.Add("PRIMER NIVEL");

            try
            {
                dtDatos = objTkt.Listar_Tickets(dtFecIni, dtFecFin, lsEstado, lsGrupo);
                //dt = objTpE.Listar_TiemposPorEstado(new DateTime(DateTime.Today.Year, 2, 1), new DateTime(DateTime.Today.Year, 2, 29, 23, 59, 59), lsEstado, lsGrupo);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de tickets de nivel 1.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de tickets de nivel 1.", ex);
            }
            return dtDatos;
        }

        private DataTable GenerarTablaRpt()
        {
            DataTable dtRep = new DataTable("REPORTE_NIVEL_1");
            dtRep.Columns.Add("SLA", typeof(int));
            dtRep.Columns.Add("Total_Tickets", typeof(int));
            dtRep.Columns.Add("Cumple_SLA", typeof(int));
            dtRep.Columns.Add("Porcentaje", typeof(int));
            DataRow drRep = dtRep.NewRow();
            int iSla = 0;
            int.TryParse(ConfigurationSettings.AppSettings["SLA_PRIMER_NIVEL"],out iSla);
            drRep["SLA"] = iSla;
            dtRep.Rows.Add(drRep);
            return dtRep;
        }
               
        private void CalcularTotales(ref DataTable i_dtResult, DataTable i_dtDatos)
        {
            DataTable dtFiltrada = DataHelper.Filter(i_dtDatos, "Grupo_Resolutor='PRIMER NIVEL'");
            i_dtResult.Rows[0]["Total_Tickets"] = i_dtDatos.Rows.Count;
            i_dtResult.Rows[0]["Cumple_SLA"] = dtFiltrada.Rows.Count;
            i_dtResult.Rows[0]["Porcentaje"] = (Convert.ToDouble(dtFiltrada.Rows.Count) / Convert.ToDouble(i_dtDatos.Rows.Count)) * 100;
        }
    }
}