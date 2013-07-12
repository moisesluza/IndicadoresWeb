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
using DAL;

/// <summary>
/// Summary description for BL
/// </summary>
public class IndicadorCanceladosPorDuplicidad
{
    public IndicadorCanceladosPorDuplicidad()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    private static IndicadorCanceladosPorDuplicidad objRpt = null;
    private DataTable dtDatos = null;

   
    public static IndicadorCanceladosPorDuplicidad getInstance()
    {
        if (objRpt == null)
            objRpt = new IndicadorCanceladosPorDuplicidad();
        return objRpt;
    }

    public DataTable ObtenerIndicadorCanceladosPorDuplicidad()
    {
        dtDatos = obtenerTickets();

        DataTable dtRep = GenerarTablaRpt();

        CalcularIndicadores(ref dtRep, dtDatos);

        return dtRep;
    }

    private DataTable obtenerTickets()
    {
        Ticket objTkt = null;
        DateTime dtFecIni;
        DateTime dtFecFin;
        List<String> lsTipos = null;

        objTkt = new Ticket();
        dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        dtFecFin = DateTime.Now;

        //dtFecIni = new DateTime(2012, 7, 1);
        //dtFecFin = new DateTime(2012, 8, 1);

        lsTipos = new List<string>();
        lsTipos.Add("I");
        lsTipos.Add("R");

        try
        {
            dtDatos = objTkt.Listar_TicketsCreadosoCanceladosPorDuplicidad(dtFecIni, dtFecFin, lsTipos);
        }
        catch (SqlException ex)
        {
            throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de tickets creados o cancelados x duplicidad.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de tickets creados o cancelados x duplicidad.", ex);
        }
        return dtDatos;
    }

    private DataTable GenerarTablaRpt()
    {
        DataTable dtRep = new DataTable("INDICADOR_CANCELADOS_DUPLICIDAD");
        dtRep.Columns.Add("SLA", typeof(double));
        dtRep.Columns.Add("Total_Tickets", typeof(int));
        dtRep.Columns.Add("Cumple_SLA", typeof(int));
        dtRep.Columns.Add("Porcentaje", typeof(double));
        dtRep.Columns.Add("indSLACumplido", typeof(int));

        DataRow drRep = dtRep.NewRow();
        dtRep.Rows.Add(drRep);
        return dtRep;
    }

    private void CalcularIndicadores(ref DataTable i_dtResult, DataTable i_dtDatos)
    {
        double dSla = 0;
        int iTotalTkt = 0;
        int iCumpleSLA = 0;
        double dPorc = 0;
        int iIndSLACumplido = -1;

        double.TryParse(ConfigurationManager.AppSettings["SLA_CANCELADOS_DUPLICIDAD"], out dSla);

        DataTable dtTicketsCancelados = DataHelper.Filter(i_dtDatos, "fecha_cancelado_duplicidad is not null");
        DataTable dtTicketsCreados = DataHelper.Filter(i_dtDatos, "fecha_cancelado_duplicidad is null");

        iTotalTkt = dtTicketsCreados.Rows.Count;
        iCumpleSLA = dtTicketsCancelados.Rows.Count;
        if (iTotalTkt != 0)
        {
            dPorc = (Convert.ToDouble(iCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100.00;
            dPorc = Math.Round(dPorc,2);
            iIndSLACumplido = dPorc <= dSla ? 1 : 0;
        }

        i_dtResult.Rows[0]["SLA"] = dSla;
        i_dtResult.Rows[0]["Total_Tickets"] = iTotalTkt;
        i_dtResult.Rows[0]["Cumple_SLA"] = iCumpleSLA;
        i_dtResult.Rows[0]["Porcentaje"] = dPorc;
        i_dtResult.Rows[0]["indSLACumplido"] = iIndSLACumplido;
    }
}
