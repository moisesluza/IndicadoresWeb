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
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace DAL
{
    /// <summary>
    /// Summary description for RespuestaEncuestas
    /// </summary>
    public class RespuestaEncuestas
    {
        public RespuestaEncuestas()
        {

        }

        public DataTable Listar_RespuestaEncuestas
        (
            DateTime dtFecIni,
            DateTime dtFecFin,
            List<string> lsEstados,
            List<string> lsGrupos
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");

            String squery =
                "SELECT g.last_name as Grupo, c.sequence AS SurveyAnswerSequence, c.txt AS SurveyAnswerTxt, count(cr.id) as Cantidad " +
                "FROM survey a " +
                "    inner join survey_question b ON a.id = b.owning_survey " +
                "    inner join survey_answer c ON b.id = c.own_srvy_question " +
                "    inner join call_req cr ON a.object_id = cr.id " +
                "    inner join ca_contact g ON cr.group_id = g.contact_uuid " +
                "where " +
                "	(DATEADD(ss, cr.resolve_date - 18000, '19700101') between '{0}' and '{1}' or DATEADD(ss, cr.close_date - 18000, '19700101') between '{0}' and '{1}') " +
                "	and cr.status in ('{2}') " +
                "	and g.last_name in ('{3}') " +
                "   and (c.selected = 1) and a.id = (select top 1 ss.id from survey ss where ss.object_id=a.object_id order by last_mod_dt) " +
                "group by c.sequence, c.txt, g.last_name " +
                "order by g.last_name, c.sequence";

            System.Data.Common.DbCommand cm = db.GetSqlStringCommand(string.Format(
                squery,
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),//Fecha con hora en formato de 24Horas
                string.Join("','", lsEstados.ToArray()),
                string.Join("','", lsGrupos.ToArray())
            ));

            try
            {
                _dt = db.ExecuteDataSet(cm).Tables[0];
            }
            catch (SqlException ex)
            {
                throw ex;
            }

            return _dt;
        }
    }
}