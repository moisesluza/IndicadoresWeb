using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using DAL;

namespace BL
{
    /// <summary>
    /// Summary description for ReporteIndicadoresLlamadas
    /// </summary>
    public class ReporteIndicadoresLlamadas
    {
        private static ReporteIndicadoresLlamadas objRpt = null;

        private ReporteIndicadoresLlamadas()
        {
            
        }

        public static ReporteIndicadoresLlamadas getInstance()
        {
            if (objRpt == null)
                objRpt = new ReporteIndicadoresLlamadas();
            return objRpt;
        }

        public DataTable ObtenerReporteIndicadoresLlamadas()
        {
            DataTable dtDatos;
            DataTable dtReporte = new DataTable("INDICADORES_LLAMADAS");

            dtDatos = ObtenerDetalleLlamadas();

            if (dtDatos.Rows.Count != 0)
            {
                dtReporte = crearTablaIndicadores();

                dtDatos = ObtenerResumenLlamadas(dtDatos);

                calcularIndicadoresReporte(ref dtReporte, dtDatos);
            }

            return dtReporte;
        }

        /// <summary>
        /// obtiene la lista detallada de llamadas del mes
        /// </summary>
        /// <returns>DataTable. Columnas: estado(2:contestado,1:abandonada), t_cola (tiempo en espera), t_talk (tiempo conversación)</returns>
        private DataTable ObtenerDetalleLlamadas()
        {
            Llamadas objRpta = null;
            DateTime dtFecIni;
            DateTime dtFecFin;
            DataTable dtDatos;
            
            objRpta = new Llamadas();
            dtFecIni = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtFecFin = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);

            //dtFecIni = new DateTime(2012, 8, 1);
            //dtFecFin = new DateTime(2012, 8, 28);
            
            try
            {
                dtDatos = objRpta.Listar_Llamadas(dtFecIni, dtFecFin,8,19);
            }
            catch (SqlException ex)
            {
                throw new Exception("Ocurrió un error con la Base de datos cuando se intentaba obtener el listado de llamadas.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error no controlado cuando se intentaba obtener el listado de llamadas.", ex);
            }
            return dtDatos;
        }

        /// <summary>
        /// Agrupa el detalle de llamadas en una tabla resumen para simplificar los datos
        /// </summary>
        /// <param name="dtDatos">Detalle de llamadas</param>
        /// <returns>DataTable con columnas: estado|TotalLlamadas|CantLlamadasTEsperaMenor|CantLlamadasTEsperaMayor|CantLlamadasTConvMayor</returns>
        private DataTable ObtenerResumenLlamadas(DataTable dtDatos)
        {
            int iTiempoMaxEspera = 0;
            int iTiempoMaxConversacion = 0;
            int iTiempoEsperaLlamada = 0;
            int iTiempoConversacionLlamada = 0;

            int.TryParse(ConfigurationManager.AppSettings["LLAMADAS_TIEMPO_ESPERA"].ToString(), out iTiempoMaxEspera);
            int.TryParse(ConfigurationManager.AppSettings["LLAMADAS_TIEMPO_CONVERSACION"].ToString(), out iTiempoMaxConversacion);

            //se convierte el tiempo de conversación a segundos
            iTiempoMaxConversacion = iTiempoMaxConversacion * 60;
            
            /*************************************/
            //Se marcan las llamadas que:
            //  tienen mayor tiempo de espera que el máximo permitido
            //  tienen menor tiempo de espera que el máximo permitido
            //  tienen mayor tiempo de conversación que el máximo permitido
            /*************************************/
            //Se agregan las columnas de marca
            dtDatos.Columns.Add(new DataColumn("TEsperaMayor", typeof(int)));
            dtDatos.Columns.Add(new DataColumn("TEsperaMenor",typeof(int)));
            dtDatos.Columns.Add(new DataColumn("TConvMenor", typeof(int)));
            
            //Se marcan las columnas
            foreach (DataRow dr in dtDatos.Rows)
            {
                int.TryParse(dr["t_cola"].ToString(), out iTiempoEsperaLlamada);
                int.TryParse(dr["t_talk"].ToString(), out iTiempoConversacionLlamada);

                if (iTiempoEsperaLlamada < iTiempoMaxEspera)
                    dr["TEsperaMenor"] = 1;
                else
                    dr["TEsperaMenor"] = 0;

                if (iTiempoEsperaLlamada >= iTiempoMaxEspera)
                    dr["TEsperaMayor"] = 1;
                else
                    dr["TEsperaMayor"] = 0;

                if (iTiempoConversacionLlamada < iTiempoMaxConversacion)
                    dr["TConvMenor"] = 1;
                else
                    dr["TConvMenor"] = 0;
            }
            /*************************************/
            //Se agrupa la información por estado
            /*************************************/
            //se hace distinct por estado
            DataTable dtGroup = DataHelper.Distinct(dtDatos, new string[] { "estado" }, "estado");

            //se agregan las columnas de totales
            dtGroup.Columns.Add(new DataColumn("TotalLlamadas", typeof(int)));
            dtGroup.Columns.Add(new DataColumn("CantLlamadasTEsperaMenor", typeof(int)));
            dtGroup.Columns.Add(new DataColumn("CantLlamadasTEsperaMayor", typeof(int)));
            dtGroup.Columns.Add(new DataColumn("CantLlamadasTConvMenor", typeof(int)));

            //Se calculan los totales
            foreach (DataRow dr in dtGroup.Rows)
            {
                dr["TotalLlamadas"] = dtDatos.Compute("count(estado)", string.Format("estado={0}", dr["estado"]));
                dr["CantLlamadasTEsperaMenor"] = dtDatos.Compute("sum(TEsperaMenor)", string.Format("estado={0}", dr["estado"]));
                dr["CantLlamadasTEsperaMayor"] = dtDatos.Compute("sum(TEsperaMayor)", string.Format("estado={0}", dr["estado"]));
                dr["CantLlamadasTConvMenor"] = dtDatos.Compute("sum(TConvMenor)", string.Format("estado={0}", dr["estado"]));
            }

            return dtGroup;
        }

        /// <summary>
        /// Crea la tabla que será devuelta a la página para mostrar los indicadores
        /// </summary>
        /// <param name="dtDatos">tabla con la información de llamadas resumida por estado</param>
        /// <returns>DataTable con columnas: NombreIndicador|SLA|Total_Llamadas|Cumple_SLA|Porcentaje</returns>
        private DataTable crearTablaIndicadores()
        {
            int iSlaContestarLlamadas = 0;
            int iSlaTasaAbandono =0;
            int iSlaTAtencion1erNivel=0;
            DataRow dr=null;

            //se obtienen los SLA's
            int.TryParse(ConfigurationManager.AppSettings["SLA_CONTESTAR_LLAMADA"],out iSlaContestarLlamadas);
            int.TryParse(ConfigurationManager.AppSettings["SLA_TASA_ABANDONO"],out iSlaTasaAbandono);
            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_ATENCION_1ER_NIVEL"],out iSlaTAtencion1erNivel);

            DataTable dtRep = new DataTable("INDICADORES_LLAMADAS");

            //columnas
            dtRep.Columns.Add(new DataColumn("NombreIndicador",typeof(string)));
            dtRep.Columns.Add(new DataColumn("SLA", typeof(int)));
            dtRep.Columns.Add(new DataColumn("Total_Llamadas", typeof(int)));
            dtRep.Columns.Add(new DataColumn("Cumple_SLA", typeof(int)));
            dtRep.Columns.Add(new DataColumn("Porcentaje", typeof(double)));
            dtRep.Columns.Add(new DataColumn("indSLACumplido", typeof(int)));
            
            //filas
            dr = dtRep.NewRow();
            dr["NombreIndicador"] = "TIEMPO_CONTESTAR_LLAMADA";
            dr["SLA"] = iSlaContestarLlamadas;
            dtRep.Rows.Add(dr);

            dr = dtRep.NewRow();
            dr["NombreIndicador"] = "TASA_ABANDONO";
            dr["SLA"] = iSlaTasaAbandono;
            dtRep.Rows.Add(dr);

            dr = dtRep.NewRow();
            dr["NombreIndicador"] = "TIEMPO_ATENCION_1ER_NIVEL";
            dr["SLA"] = iSlaTAtencion1erNivel;
            dtRep.Rows.Add(dr);

            return dtRep;
        }

        /// <summary>
        /// Calcula los Indicadores y los coloca en la tabla de reportes
        /// </summary>
        /// <param name="dtRep">tabla de llenada con los indicadores calculados</param>
        /// <param name="dtDatos">tabla con información resumida por estado</param>
        private void calcularIndicadoresReporte(ref DataTable dtRep, DataTable dtDatos)
        {
            int iSlaContestarLlamadas = 0;
            int iSlaTasaAbandono =0;
            int iSlaTAtencion1erNivel=0;

            //se obtienen los SLA's
            int.TryParse(ConfigurationManager.AppSettings["SLA_CONTESTAR_LLAMADA"],out iSlaContestarLlamadas);
            int.TryParse(ConfigurationManager.AppSettings["SLA_TASA_ABANDONO"],out iSlaTasaAbandono);
            int.TryParse(ConfigurationManager.AppSettings["SLA_TIEMPO_ATENCION_1ER_NIVEL"],out iSlaTAtencion1erNivel);

            /***********************************************************/
            //Se calcula el total de llamadas para todos los indicadores
            /***********************************************************/
            int iTotalLlamadas = 0;
            int iTotalLlamadasContestadas = 0;
            int iTotalLlamadasAbandonadasValidas = 0;
            DataRow[] dr = null;
            
            //total llamadas contestadas
            dr = dtDatos.Select("estado=2");
            if (dr.Length > 0)
                int.TryParse(dr[0]["TotalLlamadas"].ToString(), out iTotalLlamadasContestadas);
            
            //total llamadas abandonadas válidas
            int.TryParse(dtDatos.Compute("sum(CantLlamadasTEsperaMayor)", "estado<>2").ToString(), out iTotalLlamadasAbandonadasValidas); 

            //se calcula el total
            iTotalLlamadas = iTotalLlamadasContestadas + iTotalLlamadasAbandonadasValidas;

            /***************************************************/
            /*SLA: Tiempo para Contestar una llamada Telefónica*/
            /***************************************************/
            double dPorc_LlamadasContestadas = 0;
            int iTotalLlamadasContestadasAntesTiempoEspera = 0;
            
            //se calcula la cantidad de llamadas que cumplen con el SLA
            dr = dtDatos.Select("estado=2");
            if (dr.Length > 0)
                int.TryParse(dr[0]["CantLlamadasTEsperaMenor"].ToString(), out iTotalLlamadasContestadasAntesTiempoEspera);
            
            //se calcula el sla
            if (iTotalLlamadas != 0) 
            { 
                dPorc_LlamadasContestadas = (Convert.ToDouble(iTotalLlamadasContestadasAntesTiempoEspera) / Convert.ToDouble(iTotalLlamadasContestadas)) * 100;
                dPorc_LlamadasContestadas = Math.Round(dPorc_LlamadasContestadas, 2);
            }
            //se colocan los datos en la tabla
            dtRep.Rows[0]["Total_Llamadas"] = iTotalLlamadasContestadas;
            dtRep.Rows[0]["Cumple_SLA"] = iTotalLlamadasContestadasAntesTiempoEspera;
            dtRep.Rows[0]["Porcentaje"] = dPorc_LlamadasContestadas;
            dtRep.Rows[0]["indSLACumplido"] = dPorc_LlamadasContestadas >= iSlaContestarLlamadas ? 1 : 0;
            
            /***********************/
            /*SLA: Tasa de abandono*/
            /***********************/
            double dPorc_TasaAbandono = 0;
            
            //se calcula el sla
            if (iTotalLlamadas != 0)
            {
                dPorc_TasaAbandono = (Convert.ToDouble(iTotalLlamadasAbandonadasValidas) / Convert.ToDouble(iTotalLlamadas)) * 100;
                dPorc_TasaAbandono = Math.Round(dPorc_TasaAbandono, 2);
            }

            //se colocan los datos en la tabla
            dtRep.Rows[1]["Total_Llamadas"] = iTotalLlamadas;
            dtRep.Rows[1]["Cumple_SLA"] = iTotalLlamadasAbandonadasValidas;
            dtRep.Rows[1]["Porcentaje"] = dPorc_TasaAbandono;
            dtRep.Rows[1]["indSLACumplido"] = dPorc_TasaAbandono <= iSlaTasaAbandono ? 1 : 0;
            
            /******************************************/
            /*SLA: Tiempo de Atención del primer nivel*/
            /******************************************/
            double dPorc_TiempoAtencion1erNivel = 0;
            int i_LlamadasDuracionMenorATiempoConversacion = 0;
            
            //se calcula la cantidad de llamadas que cumplen con el SLA
            dr = dtDatos.Select("estado=2");
            if (dr.Length > 0)
                int.TryParse(dr[0]["CantLlamadasTConvMenor"].ToString(), out i_LlamadasDuracionMenorATiempoConversacion);
            
            //se calcula el sla
            if (iTotalLlamadasContestadas != 0)
            {
                dPorc_TiempoAtencion1erNivel = (Convert.ToDouble(i_LlamadasDuracionMenorATiempoConversacion) / Convert.ToDouble(iTotalLlamadasContestadas)) * 100.00;
                dPorc_TiempoAtencion1erNivel = Math.Round(dPorc_TiempoAtencion1erNivel, 2);
            }
            //se colocan los datos en la tabla
            dtRep.Rows[2]["Total_Llamadas"] = iTotalLlamadasContestadas;
            dtRep.Rows[2]["Cumple_SLA"] = i_LlamadasDuracionMenorATiempoConversacion;
            dtRep.Rows[2]["Porcentaje"] = dPorc_TiempoAtencion1erNivel;
            dtRep.Rows[2]["indSLACumplido"] = dPorc_TiempoAtencion1erNivel >= iSlaTAtencion1erNivel ? 1 : 0;

        }

        
    }
}