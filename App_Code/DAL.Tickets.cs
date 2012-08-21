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

            String squery =
                "select " +
                "	obj_id, " +
                "	l.location_name as Sede_Usuario, " +
                "	left(l.location_name,2) as Tipo_Sede_Usuario, " +
                "	g.last_name AS Grupo_Asignado, " +
                "	p.sym AS Prioridad, " +
                "   case " +
		        "       when p.sym = '0' and field_value in ('Asignado','Registrado') and left(l.location_name,2)='OP' then 20*60 " +
                "       when p.sym = '0' and field_value = 'En Proceso' and left(l.location_name,2)='OP' then 50*60 " +
                "       when p.sym = '1' and field_value in ('Asignado','Registrado') and left(l.location_name,2)='OP' then 35*60 " +
                "       when p.sym = '1' and field_value = 'En Proceso' and left(l.location_name,2)='OP' then 70*60 " +
                "       when p.sym = '2' and field_value in ('Asignado','Registrado') and left(l.location_name,2)='OP' then 40*60 " +
                "       when p.sym = '2' and field_value = 'En Proceso' and left(l.location_name,2)='OP' then 90*60 " +
                "       when p.sym = '3' and field_value in ('Asignado','Registrado') and left(l.location_name,2)='OP' then 50*60 " +
                "       when p.sym = '3' and field_value = 'En Proceso' and left(l.location_name,2)='OP' then 100*60 " +
                "       when p.sym = '4' and field_value in ('Asignado','Registrado') and left(l.location_name,2)='OP' then 70*60 " +
                "       when p.sym = '4' and field_value = 'En Proceso' and left(l.location_name,2)='OP' then 130*60 " +
                "       when left(l.location_name,2) = 'OD' and field_value in ('Asignado','Registrado') then 120*60 " +
                "       when left(l.location_name,2) = 'OD' and field_value = 'En Proceso' then 120*60 " +
                "       when left(l.location_name,2) = 'OR' and field_value in ('Asignado','Registrado') then 60*60 " +
                "       when left(l.location_name,2) = 'OR' and field_value = 'En Proceso' then 120*60 " +
		        "       else 0 " +
	            "   end as Tiempo_Minimo, " +
                "	field_value as Estado, " +
                "	sum(dbo.DIFFTIME_segundos(DATEADD(ss, prev_time - 18000, '19700101'),DATEADD(ss, end_time - 18000, '19700101'))) as tiempo " +
                "from usp_kpi_ticket_data kpi with(nolock)" +
                "	inner join call_req cr with(nolock) on kpi.obj_id=cr.id " +
                "	inner join ca_contact c with(nolock) ON  c.contact_uuid = cr.customer " +
                "	inner join ca_location l with(nolock) ON c.location_uuid = l.location_uuid " +
                "	inner join ca_contact g with(nolock) ON cr.group_id = g.contact_uuid " +
                "	inner join pri p with(nolock) ON cr.priority = p.enum " +
                "where field_name='status' and " +
                "	(DATEADD(ss, cr.resolve_date - 18000, '19700101') between '{0}' and '{1}' or DATEADD(ss, cr.close_date - 18000, '19700101') between '{0}' and '{1}') " +
                "	and cr.status in ('{2}') " +
                "	and g.last_name in ('{3}') " +
                "   and cr.type='I' " +
                //se filtran los tickets que aun no tienen prev_time ya que no se les puede calcular tiempo
                "   and obj_id not in (select distinct obj_id from usp_kpi_ticket_data where prev_time is null) " +
                "group by obj_id, field_value, l.location_name, g.last_name, p.sym " +
                "order by obj_id";

            System.Data.Common.DbCommand cm = db.GetSqlStringCommand(string.Format(
                squery, 
                dtFecIni.ToString("yyyy-MM-dd"), 
                dtFecFin.ToString("yyyy-MM-dd HH:mm:ss"),//Fecha con hora en formato de 24Horas
                string.Join("','",lsEstados.ToArray()),
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

        public DataTable Listar_Tickets(
            DateTime dtFecIni,
            DateTime dtFecFin,
            List<string> lsEstados,
            List<string> lsGrupos
        )
        {
            DataTable _dt = null;
            Database db = DatabaseFactory.CreateDatabase("MDB");

            String squery =
                "select cr.id, ctg.sym as Categoria_Ticket, g.last_name as Grupo_Resolutor " +
                "from call_req cr " +
                "    inner join ca_contact g with(nolock) ON cr.group_id = g.contact_uuid " +
                "    inner join prob_ctg ctg with(nolock) ON cr.category = ctg.persid " +
                "    inner join ca_contact gctg with(nolock) ON ctg.group_id = gctg.contact_uuid " +
                "where " +
                "	(DATEADD(ss, cr.resolve_date - 18000, '19700101') between '{0}' and '{1}' or DATEADD(ss, cr.close_date - 18000, '19700101') between '{0}' and '{1}') " +
                "	and cr.status in ('{2}') " +
                "	and gctg.last_name in ('{3}') " +
                "   and cr.type='I' ";

            DbCommand cm = db.GetSqlStringCommand(string.Format(
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
