using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;

/*
 * SE CREA PARA PROPORCIONAR UN REPORTE DONDE EL PROYECTO PUEDA VER LOS TICKETS QUE NO TIENE TIEMPO DE SOLUCIÓN.
 * ESTE REPORTE ES UN REQUERIMIENTO PUNTUAL, POR ELLO NO SE CONSIDERA DENTRO DE LA LÓGICA DEL TABLERO.
 */
public partial class util_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        GridView1.DataSource = RptTicketsSinTiempoSolucion();
        GridView1.DataBind();
    }

    private DataTable RptTicketsSinTiempoSolucion()
    {
        DataTable _dt = null;
        String query =
            "select t1.ref_num,t1.fecha_apertura,t1.Tipo_Sede_Usuario,t1.Sede_Usuario, t1.Grupo_Asignado, t1.Prioridad " +
            "from  " +
            "( " +
	        "    select  " +
		    "        cr.id,cr.ref_num, " +
		    "        l.location_name as Sede_Usuario, " +
		    "        left(l.location_name,2) as Tipo_Sede_Usuario, " +
		    "        g.last_name AS Grupo_Asignado, " +
		    "        p.sym AS Prioridad, " +
            "        DATEADD(ss, cr.open_date - 18000, '19700101') as fecha_apertura " +
            "    from call_req cr with(nolock) " +
            "        inner join ca_contact c with(nolock) ON  c.contact_uuid = cr.customer " +
            "        inner join ca_location l with(nolock) ON c.location_uuid = l.location_uuid " +
            "        inner join ca_contact g with(nolock) ON cr.group_id = g.contact_uuid " +
            "        inner join pri p with(nolock) ON cr.priority = p.enum " +
	        "    where " +
            "        ((DATEADD(ss, cr.resolve_date - 18000, '19700101') between '{0}' and '{1}') or (DATEADD(ss, cr.close_date - 18000, '19700101') between '{0}' and '{1}')) " +
		    "        and cr.status in ('CL','RE') " +
		    "        and g.last_name in ('SOPORTE EN SITIO - LIMA','SOPORTE EN SITIO - PROV') " +
		    "        and cr.type='I' " +
            ") as t1 left join ( " +
	        "    select  " +
		    "        obj_id, " +
		    "        l.location_name as Sede_Usuario, " +
		    "        left(l.location_name,2) as Tipo_Sede_Usuario, " +
		    "        g.last_name AS Grupo_Asignado, " +
		    "        p.sym AS Prioridad, " +
		    "        field_value " +
	        "    from usp_kpi_ticket_data kpi with(nolock) " +
            "        inner join call_req cr with(nolock) on kpi.obj_id=cr.id " +
            "        inner join ca_contact c with(nolock) ON  c.contact_uuid = cr.customer " +
            "        inner join ca_location l with(nolock) ON c.location_uuid = l.location_uuid " +
            "        inner join ca_contact g with(nolock) ON cr.group_id = g.contact_uuid " +
            "        inner join pri p with(nolock) ON cr.priority = p.enum " +
	        "    where field_name='status' and  " +
            "        ((DATEADD(ss, cr.resolve_date - 18000, '19700101') between '{0}' and '{1}') or (DATEADD(ss, cr.close_date - 18000, '19700101') between '{0}' and '{1}')) " +
		    "        and cr.status in ('CL','RE') " +
		    "        and g.last_name in ('SOPORTE EN SITIO - LIMA','SOPORTE EN SITIO - PROV') " +
		    "        and cr.type='I' and field_value = 'En proceso' " +
	        "    group by obj_id, l.location_name, g.last_name, p.sym,cr.ref_num,field_value " +
            ") as t2 on t1.id=t2.obj_id " +
            "where field_value is null " +
            "order by t1.id";

        query = String.Format(
            query, 
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd"), 
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        Database db = DatabaseFactory.CreateDatabase("MDB");
        DbCommand cm = db.GetSqlStringCommand(query);
        
        _dt = db.ExecuteDataSet(cm).Tables[0];

        return _dt;
    }
}
