using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for BL
    /// </summary>
    public class ReporteTicketsReabiertos
    {
        private static ReporteTicketsReabiertos objRpt = null;

        public ReporteTicketsReabiertos()
        {
        }

        private int _MesCalculo;
        private int _AnioCalculo;
        public int MesCalculo{get{return _MesCalculo;}set{_MesCalculo = value;}}
        public int AnioCalculo {get{return _AnioCalculo;}set{_AnioCalculo = value;}}
        private DateTime getFecIni() { return new DateTime(this.AnioCalculo, this.MesCalculo, 1); }      
        private DateTime getFecFin(){return getFecIni().AddMonths(1).AddDays(-1);}

        public static ReporteTicketsReabiertos getInstance()
        {
            if (objRpt == null)
            {
                objRpt = new ReporteTicketsReabiertos();
            }

            return objRpt;
        }

        public DataTable ObtenerRtpReabiertos()
        {
            DataTable dtRep = GenerarTablaRpt();
            DataTable dtDatos = null;

            if (MesCalculo == 0 || AnioCalculo ==0)
                return dtRep;

            dtDatos = obtenerTicketsCerrados();

            CalcularIndicadores(ref dtRep, dtDatos);

            return dtRep;
        }

        //obtiene tikets cuya primer cierre fue en el mes
        private DataTable obtenerTicketsCerrados()
        {
            DataTable dtDatos = null;
            Ticket objTkt = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            
            objTkt = new Ticket();
            dtFecIni = getFecIni();
            dtFecFin = getFecFin();

            //dtFecIni = new DateTime(2012, 7, 1);
            //dtFecFin = new DateTime(2012, 8, 1);

            try
            {
                dtDatos = objTkt.Listar_Ticketsx1eraFechaCierre(dtFecIni, dtFecFin);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de tickets (obtenerTicketsCerrados).", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de tickets (obtenerTicketsCerrados).", ex);
            }
            return dtDatos;
        }

        private DataTable obtenerTicketsReabiertos()
        {
            DataTable dtDatos = null;
            Ticket objTkt = null;
            DateTime dtFecIni;
            DateTime dtFecFin;

            objTkt = new Ticket();
            dtFecIni = getFecIni();
            dtFecFin = getFecFin();

            //dtFecIni = new DateTime(2012, 7, 1);
            //dtFecFin = new DateTime(2012, 8, 1);

            try
            {
                dtDatos = objTkt.Listar_TicketsReabiertos(dtFecIni, dtFecFin);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de tickets (obtenerTicketsReabiertos).", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de tickets (obtenerTicketsReabiertos).", ex);
            }
            return dtDatos;
        }

        private DataTable GenerarTablaRpt()
        {
            DataTable dtRep = new DataTable("REPORTE_REABIERTOS_" + (getFecIni().Month == DateTime.Today.Month ? "MES_ACTUAL" : "MES_ANTERIOR"));
            dtRep.Columns.Add("SLA", typeof(double));
            dtRep.Columns.Add("Total_Tickets", typeof(int));
            dtRep.Columns.Add("Cumple_SLA", typeof(int));
            dtRep.Columns.Add("Porcentaje", typeof(int));
            dtRep.Columns.Add("indSLACumplido", typeof(int));
            dtRep.Columns.Add("Mes", typeof(string));

            DataRow drRep = dtRep.NewRow();
            dtRep.Rows.Add(drRep);

            dtRep.Rows[0]["Mes"] = getFecIni().ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("es-pe")).ToUpper();

            return dtRep;
        }

        private void CalcularIndicadores(ref DataTable i_dtResult, DataTable i_dtDatos)
        {
            double dSla = 0;
            int iTotalTkt = 0;
            int iCumpleSLA = 0;
            double dPorc = 0;
            int iIndSLACumplido = -1;

            double.TryParse(ConfigurationManager.AppSettings["SLA_REABIERTOS"].ToString(), out dSla);

            DataTable dtReabiertos = obtenerTicketsReabiertos();

            iTotalTkt = i_dtDatos.Rows.Count;
            iCumpleSLA = dtReabiertos.Rows.Count;
            if (iTotalTkt != 0)
            {
                dPorc = (Convert.ToDouble(iCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
                dPorc = Math.Round(dPorc);
                iIndSLACumplido = dPorc <= dSla ? 1 : 0;
            }
            i_dtResult.Rows[0]["SLA"] = dSla;
            i_dtResult.Rows[0]["Total_Tickets"] = iTotalTkt;
            i_dtResult.Rows[0]["Cumple_SLA"] = iCumpleSLA;
            i_dtResult.Rows[0]["Porcentaje"] = dPorc;
            i_dtResult.Rows[0]["indSLACumplido"] = iIndSLACumplido;
        }

        
    }
}
