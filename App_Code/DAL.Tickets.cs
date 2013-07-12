using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace DAL
{
    public class Ticket
    {
        public Ticket()
        {
        }
        
        public DataTable Listar_TiemposPorEstado
        (
            DateTime dtFecIni,
            DateTime dtFecFin,
            List<string> lsEstados,
            List<string> lsGrupos,
            List<string> lsCategorias,
            string sTipo
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
                "usp_obtener_tiempos_por_estado_tickets",
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),//Fecha con hora en formato de 24Horas
                lsEstados.Count > 0 ? string.Join(",", lsEstados.ToArray()) : string.Empty,
                lsGrupos.Count > 0 ? string.Join(",", lsGrupos.ToArray()) : string.Empty,
                lsCategorias.Count > 0 ? string.Join(",", lsCategorias.ToArray()) : string.Empty,
                string.IsNullOrEmpty(sTipo) ? string.Empty : sTipo
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

        public DataTable Listar_Tickets(
            DateTime dtFecIni,
            DateTime dtFecFin,
            List<string> lsEstados,
            List<string> lsGrupos
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");
            
            DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_tickets",
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

        public DataTable Listar_TicketsCreadosoCanceladosPorDuplicidad(
            DateTime dtFecIni,
            DateTime dtFecFin,
            List<string> lsTipos
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");

            DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_tickets_CreadosyCanceladosxDuplic",
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),//Fecha con hora en formato de 24Horas
                string.Join(",", lsTipos.ToArray())
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

        public DataTable Listar_Ticketsx1eraFechaCierre(
            DateTime dtFecIni,
            DateTime dtFecFin
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");

            DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_ticketsXprimerafechacierre",
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd")//Fecha con hora en formato de 24Horas
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

        public DataTable Listar_TicketsReabiertos(
            DateTime dtFecIni,
            DateTime dtFecFin
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");

            DbCommand cm = db.GetStoredProcCommand(
                "usp_obtener_tickets_reabiertos",
                dtFecIni.ToString("yyyy-MM-dd"),
                dtFecFin.ToString("yyyy-MM-dd")//Fecha con hora en formato de 24Horas
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
