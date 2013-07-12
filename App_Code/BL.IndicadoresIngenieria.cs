using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for BL
    /// </summary>
    public class IndicadoresIngenieria
    {
        private static IndicadoresIngenieria objThis = null;
        private DataTable dtDatos = null;
        private DataTable dtTO = null;
        private enum Tiempo { Respuesta, Solucion };

        public static IndicadoresIngenieria getInstance()
        {
            if (objThis == null)
                objThis = new IndicadoresIngenieria();
            return objThis;
        }

        public IndicadoresIngenieria(){}

        public void obtenerTickets()
        {
            Ticket objTkt = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            List<string> lsGrupo = null;
            List<string> lsEstado = null;

            objTkt = new Ticket();
            dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtFecFin = DateTime.Now;

            //dtFecIni = new DateTime(2013, 1, 1);
            //dtFecFin = new DateTime(2013, 2, 1);

            lsEstado = new List<string>();
            lsEstado.Add("CL");
            lsEstado.Add("RE");

            lsGrupo = new List<string>();
            lsGrupo.Add("INGENIERÍA");
            lsGrupo.Add("PRODUCCION");

            try
            {
                dtDatos = objTkt.Listar_TiemposPorEstado(dtFecIni, dtFecFin, lsEstado, lsGrupo, new List<string>(), "I");
                //Se filtran las prioridades que no pertenecen a Ingeniería
                dtDatos = DataHelper.Filter(dtDatos, "Prioridad in ('0-l','1-l','2-l','3-l','0-lfh','1-lfh','2-lfh','3-lfh')");
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de tickets de ingeniería.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de tickets de ingeniería.", ex);
            }
        }
        
        public DataTable ObtenerIndicadoresTS()
        {
            //Crea un dataTable con los tiempos de solución de cada ticket
            DataTable dtTiempos=CalcularTSTickets();
                       
            //Calcula indicadores de cumplimiento de cada ticket
            CalcularCumplimiento(ref dtTiempos, Tiempo.Solucion);
            
            //Genera la tabla donde se almacenarán los indicadores resultantes
            DataTable dtRep = GenerarTablaRpt();

            //Calcula los indicadores
            CalcularIndicadores(ref dtRep, dtTiempos);

            dtRep.TableName = "TS_INGENIERIA";
            return dtRep;
        }

        public DataTable ObtenerIndicadoresTR()
        {
            //Crea un dataTable con los tiempos de respuesta de cada ticket
            DataTable dtTiempos = CalcularTRTickets();

            //Calcula indicadores de cumplimiento de cada ticket
            CalcularCumplimiento(ref dtTiempos, Tiempo.Respuesta);

            //Genera la tabla donde se almacenarán los indicadores resultantes
            DataTable dtRep = GenerarTablaRpt();

            //Calcula los indicadores
            CalcularIndicadores(ref dtRep, dtTiempos);
            
            dtRep.TableName = "TR_INGENIERIA";
            return dtRep;
        }

        private DataTable CalcularTSTickets()
        {
            DataTable dtTiempos = DataHelper.Distinct(dtDatos, new String[] { "obj_id", "Prioridad" }, "obj_id");
            dtTiempos.Columns.Add(new DataColumn("Tiempo", typeof(int)));

            DataTable dtFilter = null, dtTS = null;
            int iTS = 0;
            for (int i = 0; i < dtTiempos.Rows.Count; i++) 
            {
                //calcular tiempo solución
                dtFilter = DataHelper.Filter(dtDatos, "estado in ('Asignado a Ingenieria','En Proceso','Reasignado') and obj_id=" + dtTiempos.Rows[i]["obj_id"]);
                if (dtFilter != null && dtFilter.Rows.Count!=0)
                    dtTS = DataHelper.GroupBy(dtFilter, new string[] { "obj_id" }, "tiempo", "sum");
                if (dtTS != null && dtTS.Rows.Count!=0)
                    int.TryParse(dtTS.Rows[0]["TOTAL"].ToString(),out iTS);
                
                dtTiempos.Rows[i]["Tiempo"] = iTS;

                iTS = 0;
                dtTS = null;
                dtFilter = null;
            }
            return dtTiempos;
        }

        private DataTable CalcularTRTickets()
        {
            DataTable dtTiempos = DataHelper.Distinct(dtDatos, new string[] { "obj_id", "Prioridad" }, "obj_id");
            dtTiempos.Columns.Add(new DataColumn("Tiempo", typeof(int)));

            DataTable dtFilter = null, dtTR = null;
            int iTR = 0;
            for (int i = 0; i < dtTiempos.Rows.Count; i++)
            {
                //Tiempo respuesta
                dtFilter = DataHelper.Filter(dtDatos, "estado='Asignado a Ingenieria' AND obj_id=" + dtTiempos.Rows[i]["obj_id"]);
                if (dtFilter != null && dtFilter.Rows.Count!=0)
                    dtTR = DataHelper.GroupBy(dtFilter, new string[] { "obj_id" }, "tiempo", "sum");
                if (dtTR != null && dtTR.Rows.Count!=0)
                    int.TryParse(dtTR.Rows[0]["TOTAL"].ToString(), out iTR);
                
                dtTiempos.Rows[i]["Tiempo"] = iTR;

                iTR = 0;
                dtTR = null;
                dtFilter = null;
            }
            return dtTiempos;
        }
        
        /// <summary>
        /// Calcula si los tickets cumplieron o no con sus tiempos de respuesta O solucion
        /// </summary>
        /// <param name="dtTiempos">DataTable donde se asignará el tiempo a cada ticket</param>
        private void CalcularCumplimiento(ref DataTable dtTiempos, Tiempo tiempo)
        {
            TiempoObjetivoXML objTO = new TiempoObjetivoXML();
            dtTiempos.Columns.Add("Cumple", typeof(int));
            int iTiempoTicket = 0;
            double dTO = 0;
            for (int i = 0; i < dtTiempos.Rows.Count; i++)
            {
                //Se obtiene el tiempo objetivo
                if (tiempo == Tiempo.Solucion)
                    dTO=objTO.ObtenerTS(dtTiempos.Rows[i]["Prioridad"].ToString());
                else
                    dTO=objTO.ObtenerTR(dtTiempos.Rows[i]["Prioridad"].ToString());
                //Se obtiene el tiempo del ticket
                int.TryParse(dtTiempos.Rows[i]["Tiempo"].ToString(),out iTiempoTicket);
                //Se calcula el indicador de cumplimiento de tiempo de solución
                if(iTiempoTicket>dTO*60)
                    dtTiempos.Rows[i]["Cumple"] = 0;
                else
                    dtTiempos.Rows[i]["Cumple"] = 1;

                iTiempoTicket = 0;
                dTO = 0;
            }
        }

        private void CalcularIndicadores(ref DataTable dtResult, DataTable dtTiempos)
        {
            double dSla = 0;
            int iTotalTkt = 0;
            int iCumpleSLA = 0;
            double dPorc = 0;
            int iIndSLACumplido = -1;

            double.TryParse(ConfigurationManager.AppSettings["SLA_INGENIERIA"].ToString(), out dSla);

            for(int i=0; i<dtResult.Rows.Count; i++)
            {
                iTotalTkt = CalcularTotalTickets(dtResult.Rows[i]["Prioridad"].ToString());
                iCumpleSLA = CalcularTicketsDentroSLA(dtResult.Rows[i]["Prioridad"].ToString(), dtTiempos);

                if (iTotalTkt != 0)
                {
                    dPorc = (Convert.ToDouble(iCumpleSLA) / Convert.ToDouble(iTotalTkt)) * 100;
                    dPorc = Math.Round(dPorc);
                    iIndSLACumplido = (dPorc >= dSla ? 1 : 0);
                }

                dtResult.Rows[i]["SLA"] = dSla;
                dtResult.Rows[i]["Total_Tickets"] = iTotalTkt;
                dtResult.Rows[i]["Cumple_SLA"] = iCumpleSLA;
                dtResult.Rows[i]["Porcentaje"] = dPorc;
                dtResult.Rows[i]["indSLACumplido"] = iIndSLACumplido;

                iTotalTkt = 0;
                iCumpleSLA = 0;
                dPorc = 0;
                iIndSLACumplido = -1;
            }
        }
        
        private DataTable GenerarTablaRpt()
        {
            //DataTable dtRep = DataHelper.Distinct(dtDatos, new string[] { "Prioridad" }, "Prioridad");
            DataTable dtRep = new DataTable();
            dtRep.Columns.Add("Prioridad", typeof(string));
            dtRep.Columns.Add("SLA", typeof(int));
            dtRep.Columns.Add("Total_Tickets", typeof(int));
            dtRep.Columns.Add("Cumple_SLA", typeof(int));
            dtRep.Columns.Add("Porcentaje", typeof(int));
            dtRep.Columns.Add("indSLACumplido", typeof(int));

            DataRow dr = null;
            //se agregan las filas para cada prioridad 0 - 3 dentro y fuera de horario
            for (int i = 0; i < 4; i++)
            {
                //fila para prioridad dentro de horario
                dr = dtRep.NewRow();
                dr["Prioridad"] = i + "-l";
                dtRep.Rows.Add(dr);
                //fila para prioridad fuera de horario
                dr = dtRep.NewRow();
                dr["Prioridad"] = i + "-lfh";
                dtRep.Rows.Add(dr);
            }
            return dtRep;
        }

        private int CalcularTotalTickets(string sPri)
        {
            DataTable dtFiltrada =null;
            dtFiltrada = DataHelper.Filter(dtDatos, "Prioridad='" + sPri + "'");
            dtFiltrada = DataHelper.Distinct(dtFiltrada, new string[] { "obj_id" }, "obj_id");
            if (dtFiltrada != null)
                return dtFiltrada.Rows.Count;
            else
                return 0;
        }

        private int CalcularTicketsDentroSLA(string sPri, DataTable dtTiempos)
        {
            DataTable dtFiltrada = DataHelper.Filter(dtTiempos, "Cumple=1 and Prioridad='" + sPri + "'");
            if (dtFiltrada != null)
                return dtFiltrada.Rows.Count;
            else
                return 0;
        }
    }
}
