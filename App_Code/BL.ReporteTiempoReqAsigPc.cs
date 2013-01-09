using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DAL;
using System.Collections.Generic;
using System.Data.SqlClient;

/// <summary>
/// Summary description for BL
/// </summary>
public class ReporteTiempoReqAsigPc
{
    private static ReporteTiempoReqAsigPc objRpt = null;
    private DataTable dt = null;

    public ReporteTiempoReqAsigPc()
	{
	}

    public static ReporteTiempoReqAsigPc getInstance()
    {
        if (objRpt == null)
        {
            objRpt = new ReporteTiempoReqAsigPc();
        }

        return objRpt;
    }

    public DataTable ObtenerRptTiempoAsignacionEquipo()
    {
        dt = ObtenerRequerimientos();

        DataTable dtRep = GenerarTablaRpt();

        CalcularIndicadores(ref dtRep);

        return dtRep;
    }

    private DataTable GenerarTablaRpt()
    {
        DataTable dtRep = new DataTable("REPORTE_TIEMPO_ATENCION_REQS");
        dtRep.Columns.Add("SLA", typeof(int));
        dtRep.Columns.Add("Total_Tickets", typeof(int));
        dtRep.Columns.Add("Cumple_SLA", typeof(int));
        dtRep.Columns.Add("Porcentaje", typeof(int));
        dtRep.Columns.Add("indSLACumplido", typeof(int));

        DataRow drRep = dtRep.NewRow();
        dtRep.Rows.Add(drRep);
        return dtRep;
    }

    private void CalcularIndicadores(ref DataTable i_dtResult)
    {
        int iSla = 0;
        int iTotalTkt = 0;
        int iCumpleSLA = 0;
        double dPorc = 0;
        int iIndSLACumplido = -1;

        int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_ASIGNACION_EQUIPOS"], out iSla);

        iTotalTkt = CalcularTotalTickets();
        iCumpleSLA = CalcularTicketsCumplenConSLA();
        if (iTotalTkt != 0){
            dPorc = (Convert.ToDouble(iCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
            dPorc = Math.Round(dPorc);
            iIndSLACumplido = dPorc >= iSla ? 1 : 0;
        }
        i_dtResult.Rows[0]["SLA"] = iSla;
        i_dtResult.Rows[0]["Total_Tickets"] = iTotalTkt;
        i_dtResult.Rows[0]["Cumple_SLA"] = iCumpleSLA;
        i_dtResult.Rows[0]["Porcentaje"] = dPorc;
        i_dtResult.Rows[0]["indSLACumplido"] = iIndSLACumplido;
    }

    private int CalcularTotalTickets()
    {
        DataTable dtDisct = DataHelper.Distinct(dt,new String[]{"obj_id"},"obj_id");
        return dtDisct.Rows.Count;
    }

    private int CalcularTicketsCumplenConSLA()
    {
        //Se filtran los estados de los tickets
        DataTable dtFiltrada = DataHelper.Filter(dt, "Estado in ('Asignado','Devuelto a MDA','En Proceso','En Logistica','Reasignado')");

        //Se totalizan los tiempos de los estados de cada ticket
        DataTable dtAgrupada = DataHelper.GroupBy(dtFiltrada, new String[] { "obj_id" }, "tiempo", "Sum");

        int iSLO_horas = 0;
        int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_ASIGNACION_EQUIPOS"], out iSLO_horas);

        int iSLO_segundos = iSLO_horas*60*60;

        //Se filtran los tickets que cumplen
        dtFiltrada = DataHelper.Filter(dtAgrupada, "TOTAL < " + iSLO_segundos);

        return dtFiltrada.Rows.Count;
    }

    private DataTable ObtenerRequerimientos()
    {
        DataTable dt = null;
        Ticket objTpE = null;
        DateTime dtFecIni;
        DateTime dtFecFin;
        List<string> lsGrupo = null;
        List<string> lsEstado = null;
        List<string> lsCategoria = null;
        string sTipo = null;

        objTpE = new Ticket();
        dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        dtFecFin = DateTime.Now;

        //dtFecIni = new DateTime(2012, 7, 1);
        //dtFecFin = new DateTime(2012, 8, 1);

        lsEstado = new List<string>();
        lsEstado.Add("CL");
        lsEstado.Add("RE");

        lsGrupo = new List<string>();
        lsGrupo.Add("SOPORTE EN SITIO - LIMA");

        lsCategoria = new List<string>();
        lsCategoria.Add("Asignacion.PC");
        lsCategoria.Add("Asignacion.Laptop");

        sTipo = "R";

        try
        {
            dt = objTpE.Listar_TiemposPorEstado(dtFecIni, dtFecFin, lsEstado, lsGrupo, lsCategoria, sTipo);
            //Se filtran los tickets abiertos desde el 2012-09-17 porque esa es la fecha en que se activó el SLA
            dt = DataHelper.Filter(dt, "open_date > #2012-09-17#");
        }
        catch (SqlException ex)
        {
            throw new Exception("Ocurrió un error con la Base de datos cuando se intentó obtener los tiempos por estado.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un error no controlado cuando se intento obtener los tiempos por estado.", ex);
        }
        return dt;
    }

}
