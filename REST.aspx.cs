using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using BL;

using Microsoft.Practices.EnterpriseLibrary.Logging;

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

            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoRespuestaOP());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoRespuestaODyOR());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoSolucionOP());
            ds.Tables.Add(objBcRepN2.ObtenerRptTiempoSolucionODyOR());
            ds.Tables.Add(objBcRepN1.ObtenerRtpIndicadoresNivel1());
            ds.Tables.Add(objBcEnc.ObtenerReporteIndicadoresEncuestas());
            ds.Tables.Add(objBcLlam.ObtenerReporteIndicadoresLlamadas());
            
        }
        catch (Exception ex)
        {
            try
            {
                LogEntry logEntry = new LogEntry();
                logEntry.EventId = 100;
                logEntry.Priority = 2;
                logEntry.Message = ex.Message;
                logEntry.Categories.Add("Error");
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("StackTrace", ex.StackTrace);
                dictionary.Add("Method", ex.TargetSite);
                if (ex.InnerException != null)
                {
                    dictionary.Add("InnerException Message", ex.InnerException.Message);
                    dictionary.Add("InnerException StackTrace", ex.InnerException.StackTrace);
                    dictionary.Add("InnerException Method", ex.InnerException.TargetSite);
                }
                logEntry.ExtendedProperties = dictionary;
                Logger.Write(logEntry);
            }
            catch (Exception ex2)
            {
                Response.Write("<ERROR>Ocurrió un error cuando se intentaba ggrabar el log de errores.<br/>" + ex2.Message + "</ERROR>");
                Response.End();
            }

            Response.Write("<ERROR>" + ex.Message + "</ERROR>");
            Response.End();
        }
        
        Response.Write(ds.GetXml());
        Response.End();
    }
}
