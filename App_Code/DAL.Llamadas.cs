using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace DAL
{
    /// <summary>
    /// Obtiene el detalle de llamadas.
    /// </summary>
    public class Llamadas
    {
        public Llamadas()
        {
            
        }
        /// <summary>
        /// Obtiene el detalle de llamadas en un rango de fecha de fechas determinado
        /// </summary>
        /// <param name="dtFecIni">Fecha de inicio</param>
        /// <param name="dtFecFin">Fecha de fin</param>
        /// <returns>Datatable con el detalle de llamadas obtenido</returns>
        public DataTable Listar_Llamadas
        (
            DateTime dtFecIni,
            DateTime dtFecFin
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("TABLERO");

            String squery =
                "select estado,t_cola, t_talk " +
                "from detalle_llamadas " +
                "where fecha_inicio between '{0}' and '{1}'";

            System.Data.Common.DbCommand cm = db.GetSqlStringCommand(string.Format(
                squery,
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss")//Fecha con hora en formato de 24Horas
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
