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

            CalcularIndicadores(ref dtRep, dtDatos);
            
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

            //dtFecIni = new DateTime(2012, 7, 1);
            //dtFecFin = new DateTime(2012, 8, 1);

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
            dtRep.Columns.Add("indSLACumplido", typeof(int));

            DataRow drRep = dtRep.NewRow();
            dtRep.Rows.Add(drRep);
            return dtRep;
        }
               
        private void CalcularIndicadores(ref DataTable i_dtResult, DataTable i_dtDatos)
        {
            int iSla = 0;
            int iTotalTkt = 0;
            int iCumpleSLA = 0;
            double dPorc = 0;
            int iIndSLACumplido = 0;

            int.TryParse(ConfigurationManager.AppSettings["SLA_PRIMER_NIVEL"], out iSla);

            DataTable dtFiltrada = DataHelper.Filter(i_dtDatos, "Grupo_Resolutor='PRIMER NIVEL'");

            iTotalTkt = i_dtDatos.Rows.Count;
            iCumpleSLA = dtFiltrada.Rows.Count;
            if(iTotalTkt!=0)
                dPorc = (Convert.ToDouble(iCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
            dPorc = Math.Round(dPorc);
            iIndSLACumplido = dPorc >= iSla ? 1 : 0;

            i_dtResult.Rows[0]["SLA"] = iSla;
            i_dtResult.Rows[0]["Total_Tickets"] = iTotalTkt;
            i_dtResult.Rows[0]["Cumple_SLA"] = iCumpleSLA;
            i_dtResult.Rows[0]["Porcentaje"] = dPorc;
            i_dtResult.Rows[0]["indSLACumplido"] = iIndSLACumplido;
        }
    }
}