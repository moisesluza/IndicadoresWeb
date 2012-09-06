using System;
using System.Data;
using System.Configuration;
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
            Database db = null;
            try
            {
                db = DatabaseFactory.CreateDatabase("MDB");
            }
            catch (Exception ex)
            {
                throw new Exception("No se encontró la cadena de conexión para la base de datos del Service Desk (MDB). Agréguela al archivo de configuración.", ex);
            }            
           
            System.Data.Common.DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_respuesta_encuestas",
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),//Fecha con hora en formato de 24Horas
                string.Join(",", lsEstados.ToArray()),
                string.Join(",", lsGrupos.ToArray())
            );

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