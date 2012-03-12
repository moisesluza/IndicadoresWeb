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
using BL;

public partial class REST : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/xml";
        Response.Write("<?xml version='1.0' encoding='ISO-8859-1'?>");
        
        ReporteIndicadores objBc = ReporteIndicadores.getInstance();
        DataSet ds = new DataSet("DATA");

        try
        {
            objBc.obtenerTiemposPorEstado();

            ds.Tables.Add(objBc.ObtenerRptTiempoRespuestaOP());
            ds.Tables.Add(objBc.ObtenerRptTiempoRespuestaODyOR());
            ds.Tables.Add(objBc.ObtenerRptTiempoSolucionOP());
            ds.Tables.Add(objBc.ObtenerRptTiempoSolucionODyOR());
        }
        catch (Exception ex)
        {
            Response.Write("<ERROR>" + ex.Message + "</ERROR>");
            Response.End();
        }
        
        Response.Write(ds.GetXml());
        Response.End();
    }
}
