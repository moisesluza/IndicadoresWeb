using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for DAL
/// </summary>
public class TiempoObjetivoXML
{
    private DataSet dsTO = null;

    public TiempoObjetivoXML() {
        dsTO = new DataSet();
        //dsTO.ReadXml("C:\\Documents and Settings\\XPMUser\\Mis documentos\\IndicadoresWeb_Osinergmin\\webIndicadores\\TiempoObjetivoING.xml");
        dsTO.ReadXml(HttpContext.Current.Request.PhysicalApplicationPath.ToString() + "\\resources\\TiempoObjetivoING.xml");
    }

    public double ObtenerTR(string sPrioridad)
    {
        DataRow[] dr = dsTO.Tables[0].Select("Prioridad='" + sPrioridad + "'");
        if (dr == null)
            return 0.0;
        else
        {
            double dTO = 0;
            if (dr.Length != 0)
                double.TryParse(dr[0]["TiempoRespuesta"].ToString(), out dTO);
            return dTO;
        }
    }

    public double ObtenerTS(string sPrioridad)
    {
        DataRow[] dr = dsTO.Tables[0].Select("Prioridad='" + sPrioridad + "'");
        if (dr == null)
            return 0.0;
        else
        {
            double dTO = 0;
            if (dr.Length!=0)
                double.TryParse(dr[0]["TiempoSolucion"].ToString(), out dTO);
            return dTO;
        }
    }

}
