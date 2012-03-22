using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for ReporteIndicadoresEncuestas
    /// </summary>
    public class ReporteIndicadoresEncuestas
    {
        private static ReporteIndicadoresEncuestas objRpt = null;
        private DataTable dtDatos = null;

        private ReporteIndicadoresEncuestas()
        {

        }

        public static ReporteIndicadoresEncuestas getInstance()
        {
            if (objRpt == null)
                objRpt = new ReporteIndicadoresEncuestas();
            return objRpt;
        }

        public DataTable ObtenerReporteIndicadoresEncuestas()
        {
            dtDatos = ObtenerRespuestaEncuestas();

            //Se reemplazan los nombres grupos del segundo nivel (SOPORTE EN SITIO - LIMA, 
            //SOPORTE EN SITIO - PROV) por "SEGUNDO NIVEL"
            foreach (DataRow dr in dtDatos.Rows)
            {
                if (dr["Grupo"].ToString().Equals("SOPORTE EN SITIO - LIMA") || 
                    dr["Grupo"].ToString().Equals("SOPORTE EN SITIO - PROV"))
                {
                    dr["Grupo"] = "SEGUNDO NIVEL";
                }
            }

            //Agrupar por grupo y respuesta
            DataTable dtPorGrupoRpta = DataHelper.GroupBy(dtDatos, new string[] { "Grupo", "SurveyAnswerSequence" }, "Cantidad", "Sum");

            //Agrupar por grupo
            DataTable dtPorGrupo = DataHelper.Distinct(dtPorGrupoRpta, new string[] { "Grupo" }, "Grupo");

            AgregarTotales(ref dtPorGrupo);

            CalcularTotales(ref dtPorGrupo, dtPorGrupoRpta);

            dtPorGrupo.TableName = "RESPUESTA_ENCUESTAS";
            return dtPorGrupo;
        }

        private DataTable ObtenerRespuestaEncuestas()
        {
            RespuestaEncuestas objRptaEnc = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            List<String> lsGrupo = null;
            List<String> lsEstado = null;

            objRptaEnc = new RespuestaEncuestas();
            dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtFecFin = DateTime.Now;

            lsEstado = new List<string>();
            lsEstado.Add("CL");
            lsEstado.Add("RE");

            lsGrupo = new List<string>();
            lsGrupo.Add("PRIMER NIVEL");
            lsGrupo.Add("SOPORTE EN SITIO - LIMA");
            lsGrupo.Add("SOPORTE EN SITIO - PROV"); 

            try
            {
                dtDatos = objRptaEnc.Listar_RespuestaEncuestas(dtFecIni, dtFecFin, lsEstado, lsGrupo);
                //dt = objTpE.Listar_TiemposPorEstado(new DateTime(DateTime.Today.Year, 2, 1), new DateTime(DateTime.Today.Year, 2, 29, 23, 59, 59), lsEstado, lsGrupo);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de respuestas de encuestas de tickets.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de respuestas de encuestas de tickets.", ex);
            }
            return dtDatos;
        }

        private void AgregarTotales(ref DataTable i_dtOrigen)
        {
            i_dtOrigen.Columns.Add("SLA", typeof(int));
            i_dtOrigen.Columns.Add("Total_Tickets", typeof(int));
            i_dtOrigen.Columns.Add("Cumple_SLA", typeof(int));
            i_dtOrigen.Columns.Add("Porcentaje", typeof(int));
        }

        private void CalcularTotales(ref DataTable i_dtResult, DataTable i_dtDatos)
        {
            int iSlaEnc = 0;
            int.TryParse(ConfigurationSettings.AppSettings["SLA_ENCUESTAS"].ToString(),out iSlaEnc);
            foreach (DataRow dr in i_dtResult.Rows)
            {
                dr["SLA"] = iSlaEnc;
                dr["Total_Tickets"] = i_dtDatos.Compute("Sum(TOTAL)", string.Format("Grupo='{0}'", dr["Grupo"]));
                dr["Cumple_SLA"] = i_dtDatos.Compute("Sum(TOTAL)", string.Format("Grupo='{0}' and SurveyAnswerSequence in (10,20)", dr["Grupo"])); ;
                dr["Porcentaje"] = (Convert.ToDouble(dr["Cumple_SLA"].ToString()) / Convert.ToDouble(dr["Total_Tickets"].ToString())) * 100;
            }            
        }
    }
}
