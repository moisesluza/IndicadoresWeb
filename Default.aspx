<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>OSINERGMIN - Indicadores Web</title>
    <link href="style.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript" src="jquery-1.7.1.min.js"></script>
    <script type="text/javascript" language="javascript">
        var tiempo_actualizacion = <%= ConfigurationSettings.AppSettings["TIEMPO_ACTUALIZACION"] %>;
        
        $(document).ready(function(){
            update();
            $("#divTAct").html("Tiempo de Actualización: " + parseInt(tiempo_actualizacion)/1000/60 + " min");
        });
        
        function update() {
            $("#divNotice")
                .removeClass()
                .addClass("info")
                .html("Actualizando...")
                .show();
                
            $.post(
                "REST.aspx",
                {},
                function(data){
                    if($(data).find("ERROR").length > 0){
                        $("#divNotice")
                            .removeClass()
                            .addClass("error")
                            .html($(data).find("ERROR").text())
                        nuevoIntento();
                    }else{
                        mostrarData(data);
                        $("#divNotice").html("").hide();
                        window.setTimeout(update, tiempo_actualizacion);
                   }
                }
            );
        }
        
        function nuevoIntento(){
            window.setTimeout(function(){
                $("#divNotice").hide();
                var sec = <%= ConfigurationSettings.AppSettings["TIEMPO_NUEVO_INTENTO"] %>;
                var timer = setInterval(function() { 
                    $("#divNotice").removeClass().addClass('info').text("Intentando nuevamente en " + (sec--) + "seg").show();
                    if (sec == -1) {
                        $("#divNotice").hide();
                        clearInterval(timer);
                        update();
                    }
                }, 1000);
            },5000);
        }
        
        function mostrarData(data){
            mostrarDataPorPrioridad($("#tbTRptaOP"),$(data).find("TiempoRespuestaOP"));
            mostrarDataPorPrioridad($("#tbTSolOP"),$(data).find("TiempoSolucionOP"));
            mostrarDataPorOficina($("#tbTRptaODyOR"),$(data).find("TiempoRespuestaODyOR"));
            mostrarDataPorOficina($("#tbTSolODyOR"),$(data).find("TiempoSolucionODyOR"));
            mostrarDataRepNivel1($("#tbNivel1"),$(data).find("REPORTE_NIVEL_1"));
            mostrarDataEncuestas($("#tbEncuestas"),$(data).find("RESPUESTA_ENCUESTAS"));
        }
        
        function mostrarDataPorPrioridad(tb, data){
            limpiarValores(tb);
            $(data).each(function(){
                sla = parseInt($(this).find('SLA').text());
                pri = parseInt($(this).find('Prioridad').text());
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                
                //Se obtienen las celdas donde se escribirán los valores
                tr = $(tb).find("tbody tr")[pri]
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];
                var img = new Image();
                //Se escriben los valores
                $(tdSla).text(">=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                if(parseInt(porc)>=sla)
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
            });
        }
        
        function mostrarDataPorOficina(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                sla = parseInt($(this).find('SLA').text());
                tipo_sede = $(this).find('Tipo_Sede_Usuario').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                var img = new Image();
                //Se obtienen las celdas donde se escribirán los valores
                if(tipo_sede == 'OD')
                    tr = $(tb).find("tbody tr")[0]
                else
                    tr = $(tb).find("tbody tr")[1]
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];                
                //Se escriben los valores
                $(tdSla).text(">=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                if(parseInt(porc)>=sla){
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                }else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
            });
        }
        
        function mostrarDataRepNivel1(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                tr = $(tb).find("tbody tr")[0];   
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];                
                //Se escriben los valores
                $(tdSla).text(">=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                if(parseInt(porc)>=sla){
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                }else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
            });
        }
        
        function mostrarDataEncuestas(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                grupo = $(this).find('Grupo').text();
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                if(grupo == 'PRIMER NIVEL')
                    tr = $(tb).find("tbody tr")[0]
                else
                    tr = $(tb).find("tbody tr")[1]
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];
                //Se escriben los valores
                $(tdSla).text(">=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                if(parseInt(porc)>=sla){
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                }else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
            });
        }
        
        function limpiarValores(tb){
            $(tb).find("tbody tr").each(function(){
                $(this).find("td:gt(2)").text(0);
                $(this).find("td:eq(6)").text("").find("img").remove();
            });
        }
    </script>

</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div id="divNotice" class="notice">
            </div>
            <div id="header">
                <img src="img/osinerg.png" width="342" height="65" style="float:left;" alt="NO IMG" />
                <img src="img/gmd.png" width="64" height="65" style="float:right;" alt="NO IMG" />
                <h1>Indicadores del Servicio de Mesa de Ayuda</h1>
                <div style="clear: both"></div>                
            </div>
            <div style="float: left; width: 49%;">
                <table id="tbTRptaOP">
                    <thead>
                        <tr>
                            <th colspan="2">
                                Niveles de Servicio</th>
                            <th colspan="5">
                                Tiempo Respuesta - Magdalena, STOR y GART</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Prioridad</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Tickets</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="trTRptaPri0">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 0 - 20min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTRptaPri1">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 1 - 35min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTRptaPri2">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 2 - 40min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTRptaPri3">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 3 - 50min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTRptaPri4">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 4 - 70min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <br />
                <table id="tbTRptaODyOR">
                    <thead>
                        <tr>
                            <th colspan="2">
                                Niveles de Servicio</th>
                            <th colspan="5">
                                Tiempo Respuesta - Oficinas Descentralizadas y Regionales</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Oficinas</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Tickets</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="trTRptaOD">
                            <td>
                                2do Nivel</td>
                            <td>
                                Of Descentralizadas - 120min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTRptaOR">
                            <td>
                                2do Nivel</td>
                            <td>
                                Of Regionales - 60min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div style="float: right; width: 49%;">
                <table id="tbTSolOP">
                    <thead>
                        <tr>
                            <th colspan="2">
                                Niveles de Servicio</th>
                            <th colspan="5">
                                Tiempo de Solución - Magdalena, STOR y GART</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Prioridad</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Tickets</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="trTSolPri0">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 0 - 50min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTSolPri1">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 1 - 70min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTSolPri2">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 2 - 90min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTSolPri3">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 3 - 100min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTSolPri4">
                            <td>
                                2do Nivel</td>
                            <td>
                                Prioridad 4 - 130min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <br />
                <table id="tbTSolODyOR">
                    <thead>
                        <tr>
                            <th colspan="2">
                                Niveles de Servicio</th>
                            <th colspan="5">
                                Tiempo de Solución - Oficinas Descentralizadas y Regionales</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Oficinas</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Tickets</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="trTSolOD">
                            <td>
                                2do Nivel</td>
                            <td>
                                Of Descentralizadas - 120min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr id="trTSolOR">
                            <td>
                                2do Nivel</td>
                            <td>
                                Of Regionales - 120min</td>
                            <td>
                                &gt;=98%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div style="clear: both"></div>
            <br />
            <div>
                <table id="tbNivel1">
                    <thead>
                        <tr>
                            <th colspan="7" align="center">
                                Resolución de tickets - Primer nivel</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Detalle</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Tickets</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="tr1">
                            <td>
                                1er Nivel</td>
                            <td>
                                Resolución en el Primer Nivel</td>
                            <td>
                                &gt;=70%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <br />
                <table id="tbEncuestas">
                    <thead>
                        <tr>
                            <th colspan="7" align="center">
                                Satisfacción de Encuestas</th>
                        </tr>
                        <tr>
                            <th>
                                Grupo</th>
                            <th>
                                Detalle</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Encuestas</th>
                            <th>
                                Encuestas Satisfactorias</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="tr2">
                            <td>
                                1er Nivel</td>
                            <td>
                                Satisfacción de usuarios</td>
                            <td>
                                &gt;=90%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                2do Nivel</td>
                            <td>
                                Satisfacción de usuarios</td>
                            <td>
                                &gt;=90%</td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div id="divTAct" style="float:right;">Tiempo de Actualización: 10 seg</div>
            <div style="clear: both"></div>
        </div>
    </form>
</body>
</html>
