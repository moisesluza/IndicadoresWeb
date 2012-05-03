using System;
using System.Data;
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
            DateTime dtFecFin,
            int iHoraIni,
            int iHoraFin
        )
        {
            DataTable _dt = null;
            Database db = null;

            try
            {
                db = DatabaseFactory.CreateDatabase("TABLERO");
            }
            catch (Exception ex)
            {
                throw new Exception("No se encontr� la cadena de conexi�n para el TABLERO. Agr�guela al archivo de configuraci�n.",ex);
            }
            

            String squery =
                "select estado,t_cola, t_talk, fecha_inicio " +
                "from detalle_llamadas " +
                "where fecha_inicio between '{0}' and '{1}' " +
                "   and datepart(hour,fecha_inicio) between {2} and {3} " +
                "   and Datepart(weekday, fecha_inicio) not in ({4},{5})";//Se excluyen fines de semana

            System.Data.Common.DbCommand cm = db.GetSqlStringCommand(string.Format(
                squery,
                dtFecIni.ToString("yyyy-MM-dd HH:mm:ss"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),
                iHoraIni, iHoraFin-1,
                1,7//d�as que se excluyen de la medici�n (s�bado y domingo)
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
