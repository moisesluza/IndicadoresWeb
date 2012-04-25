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

        ReporteIndicadoresNivel2 objBcRepN2 = ReporteIndicadoresNivel2.getInstance();
        ReporteIndicadoresNivel1 objBcRepN1 = ReporteIndicadoresNivel1.getInstance();
        ReporteIndicadoresEncuestas objBcEnc = ReporteIndicadoresEncuestas.getInstance();
        ReporteIndicadoresLlamadas objBcLlam = ReporteIndicadoresLlamadas.getInstance();
        DataSet ds = new DataSet("DATA");

        try
        {
            objBcRepN2.obtenerTiemposPorEstado();

            /*ds.Tables.Add(objBcRepN2.ObtenerRptTiempoRespuestaOP());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoRespuestaODyOR());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoSolucionOP());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoSolucionODyOR());
            ds.Tables.Add(objBcRepN1.ObtenerRtpIndicadoresNivel1());
            ds.Tables.Add(objBcEnc.ObtenerReporteIndicadoresEncuestas());*/
            ds.Tables.Add(objBcLlam.ObtenerReporteIndicadoresLlamadas());
            
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
