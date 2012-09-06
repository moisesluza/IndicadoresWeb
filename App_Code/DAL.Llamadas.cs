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
            
            System.Data.Common.DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_llamadas",
                dtFecIni.ToString("yyyy-MM-dd HH:mm:ss"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),
                iHoraIni, iHoraFin-1);

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
