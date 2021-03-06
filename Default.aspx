﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>OSINERGMIN - Indicadores Web</title>
    <link href="css/style.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" language="javascript" src="js/jquery-1.7.1.min.js"></script>
    <script type="text/javascript" language="javascript" src="js/jshashtable-2.1.js"></script>
    <script type="text/javascript" language="javascript" src="js/jquery.numberformatter-1.2.3.min.js"></script>
    <script type="text/javascript" language="javascript">
        var tiempo_actualizacion = <%= ConfigurationManager.AppSettings["TIEMPO_ACTUALIZACION"] %>;
        
        $(document).ready(function(){
            $("#divTAct").html("Tiempo de Actualización: " + parseInt(tiempo_actualizacion)/1000/60 + " min");
            $("#tabTickets").addClass("tabs_selected");
            $("#divLlamadas").hide();
            $("#divIngenieria").hide();
            
            $("#tabs ul li").hover(function(){$(this).addClass("tabs_hover");},function(){$(this).removeClass("tabs_hover");})
            
            $("#tabs ul li").click(function(){
                $("#tabs ul li").removeClass("tabs_selected");//se eliminan las clases de todos los tab
                $(this).addClass("tabs_selected");
                
                $("#tabs ul li a").each(function(){//Se ocultan todos los paneles
                    $($(this).attr("href")).hide();
                });                
                $($(this).find("a").attr("href")).show();//semuestra el panel asociado al tab
            });
            
            //Se asocia el evento click del li cuando se hace click en el anchor
            $("#tabs ul li a").click(function(){
                $(this).parent().click();
                return false;
            });
            
            update();
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
                var sec = <%= ConfigurationManager.AppSettings["TIEMPO_NUEVO_INTENTO"] %>;
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
            mostrarDataLlamadas($("#tbIndLlamadas"),$(data).find("INDICADORES_LLAMADAS"));
            mostrarDataTAtencionReq($("#tbTAtencionReq"),$(data).find("REPORTE_TIEMPO_ATENCION_REQS"));
            
            limpiarValores($("#tbReabiertos"));
            mostrarTicketsReabiertosMesActual($("#tbReabiertos"),$(data).find("REPORTE_REABIERTOS_MES_ACTUAL"));
            mostrarTicketsReabiertosMesAnterior($("#tbReabiertos"),$(data).find("REPORTE_REABIERTOS_MES_ANTERIOR"));
            
            mostrarDataTSIng($(data).find("TS_INGENIERIA"));
            mostrarDataTRIng($(data).find("TR_INGENIERIA"));
            mostrarDataCancPorDuplic($("#tbCancelados"),$(data).find("INDICADOR_CANCELADOS_DUPLICIDAD"));
        }
        
        function mostrarDataTSIng(data){
            limpiarValoresIngenieria($("#tbTSdh"));
            limpiarValoresIngenieria($("#tbTSfh"));
            $(data).each(function(){
                sla = parseInt($(this).find('SLA').text());
                pri = parseInt($(this).find('Prioridad').text().substr(0,1));
                pri_desc=$(this).find('Prioridad').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                //Obtener celdas donde se colocaran los valores
                var tb = esFueraHorarioOficina(pri_desc) ? $("#tbTSfh") : $("#tbTSdh");
                tr = $(tb).find("tbody tr")[pri]
                tdSla = $(tr).find("td")[1];
                tdTotTkt = $(tr).find("td")[2];
                tdDentroSLA = $(tr).find("td")[3];
                tdPorc = $(tr).find("td")[4];
                tdImg = $(tr).find("td")[5];
                var img = new Image();
                //Se escriben los valores
                $(tdSla).text("=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }
            });
        }
        
        function mostrarDataTRIng(data){
            limpiarValoresIngenieria($("#tbTRdh"));
            limpiarValoresIngenieria($("#tbTRfh"));
            $(data).each(function(){
                sla = parseInt($(this).find('SLA').text());
                pri = parseInt($(this).find('Prioridad').text().substr(0,1));
                pri_desc=$(this).find('Prioridad').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                //Obtener celdas donde se colocaran los valores
                var tb = esFueraHorarioOficina(pri_desc) ? $("#tbTRfh") : $("#tbTRdh");
                tr = $(tb).find("tbody tr")[pri]
                tdSla = $(tr).find("td")[1];
                tdTotTkt = $(tr).find("td")[2];
                tdDentroSLA = $(tr).find("td")[3];
                tdPorc = $(tr).find("td")[4];
                tdImg = $(tr).find("td")[5];
                var img = new Image();
                //Se escriben los valores
                $(tdSla).text("=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }
            });
        }
        
        function esFueraHorarioOficina(pri){return pri.indexOf('lfh') != -1;}
        
        function mostrarDataPorPrioridad(tb, data){
            limpiarValores(tb);
            $(data).each(function(){
                sla = parseInt($(this).find('SLA').text());
                pri = parseInt($(this).find('Prioridad').text());
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                
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
                if(ind == 1)
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
                
            });
        }
        
        function mostrarDataTAtencionReq(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
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
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }
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
                ind = $(this).find('indSLACumplido').text();
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
                if(ind == 1)
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                else 
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
                ind = $(this).find('indSLACumplido').text();
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
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }            
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
                ind = $(this).find('indSLACumplido').text();
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
                if(ind == 1)
                    $(img).attr("src","img/verde.png").appendTo(tdImg);
                else 
                    $(img).attr("src","img/rojo.png").appendTo(tdImg);
                
            });
        }
        
       
        function mostrarTicketsReabiertosMesActual(tb, data){
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                mes = $(this).find('Mes').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                tr = $(tb).find("tbody tr")[0];
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[7];
                tdDentroSLA = $(tr).find("td")[8];
                tdPorc = $(tr).find("td")[9];
                tdImg = $(tr).find("td")[10];
                thMonthTitle = $(tb).find("thead tr th ")[2];
                //Se escriben los valores
                $(tdSla).text("<=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }
                $(thMonthTitle).text(mes);
            });
        }
        
        function mostrarTicketsReabiertosMesAnterior(tb, data){
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                mes = $(this).find('Mes').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                tr = $(tb).find("tbody tr")[0];
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];
                thMonthTitle = $(tb).find("thead tr th ")[1];
                //Se escriben los valores
                $(tdSla).text("<=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }
                $(thMonthTitle).text(mes);
            });
        }
        
        function mostrarDataLlamadas(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_llamadas = $(this).find('Total_Llamadas').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                tr = $(tb).find("tbody tr")[i];
                //se obtienen las celdas donde se colocorá la información
                tdSla = $(tr).find("td")[2];
                tdTot = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];
                //Se escriben los valores
                cond = $(tdSla).text().substr(0,2);
                $(tdSla).text(cond + sla + "%");
                $(tdTot).text(tot_llamadas);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc);
                $(tdPorc).parseNumber({format:"#,###.00", locale:"us"});
                $(tdPorc).formatNumber({format:"#,###.00", locale:"us"});
                $(tdPorc).text($(tdPorc).text() + "%");
                if(tot_llamadas!=0)
                    if(ind == 1)
                        $(img).attr("src","img/verde.png").appendTo(tdImg);
                    else
                        $(img).attr("src","img/rojo.png").appendTo(tdImg);
            });
        }
        
        function mostrarDataCancPorDuplic(tb, data){
            limpiarValores(tb);
            $(data).each(function(i){
                sla = $(this).find('SLA').text();
                tot_tickets = $(this).find('Total_Tickets').text();
                cumple_sla = $(this).find('Cumple_SLA').text();
                porc = $(this).find('Porcentaje').text();
                ind = $(this).find('indSLACumplido').text();
                var img = new Image();
                //se obtiene la fila donde se mostraran los valores
                tr = $(tb).find("tbody tr")[0];   
                tdSla = $(tr).find("td")[2];
                tdTotTkt = $(tr).find("td")[3];
                tdDentroSLA = $(tr).find("td")[4];
                tdPorc = $(tr).find("td")[5];
                tdImg = $(tr).find("td")[6];                
                //Se escriben los valores
                $(tdSla).text("<=" + sla + "%");
                $(tdTotTkt).text(tot_tickets);
                $(tdDentroSLA).text(cumple_sla);
                $(tdPorc).text(porc + "%");
                switch(ind){
                    case "1": $(img).attr("src","img/verde.png").appendTo(tdImg); break;
                    case "0": $(img).attr("src","img/rojo.png").appendTo(tdImg); break;
                    default: img = null;
                }            
            });
        }
        
        function limpiarValores(tb){
            $(tb).find("tbody tr").each(function(){
                $(this).find("td:gt(2)").text(0);
                $(this).find("td:eq(6)").text("").find("img").remove();
                $(this).find("td:eq(10)").text("").find("img").remove();
            });
        }
        
        function limpiarValoresIngenieria(tb){
            $(tb).find("tbody tr").each(function(){
                $(this).find("td:gt(1)").text(0);
                $(this).find("td:eq(5)").text("").find("img").remove();
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
                <img src="img/gmd.png" width="130" height="65" style="float:right;" alt="NO IMG" />
                <h1>Indicadores del Servicio de Mesa de Ayuda</h1>
                <div style="clear: both"></div>
            </div>
            <div id="tabs">
              <ul>
                <li id="tabTickets" class="tab"><a href="#divTickets">Tickets</a></li>
                <li id="tabLlamadas" class="tab"><a href="#divLlamadas">Llamadas</a></li>
                <li id="tabIngenieria" class="tab"><a href="#divIngenieria">Ingeniería</a></li>
              </ul>
            </div>
            <div style="clear: both"></div>
            <div id="divTickets" class="content">
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
                    <div style="float: left; width: 49%; ">
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
                    </div>
                    <div style="float: right; width: 49%; ">
                    <table id="tbTAtencionReq">
                        <thead>
                            <tr>
                                <th colspan="7" align="center">
                                    Tiempo de atención de asignación de equipo</th>
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
                            <tr id="tr3">
                                <td>
                                    SOPORTE EN SITIO - LIMA</td>
                                <td>
                                    Tiempo total de Atención para Asignaciones de Equipos</td>
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
                    <div style="clear: both"></div>
                </div>
                <br/>
                <div>
                    <table id="tbReabiertos" >
                        <thead>
                            <tr>
                                <th colspan="3" align="center">
                                    Máximo de tickets Reabiertos en el mes</th>
                                <th align="center" colspan="4">
                                    Mes anterior</th>
                                <th align="center" colspan="4">
                                    Mes actual</th>
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
                                    Tickets Reaperturados</th>
                                <th>
                                    Porcentaje</th>
                                <th>
                                    Cumplió</th>
                                <th>
                                    Total Tickets</th>
                                <th>
                                    Tickets Reaperturados</th>
                                <th>
                                    Porcentaje</th>
                                <th>
                                    Cumplio</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr id="Tr5">
                                <td>
                                    Grupos resolutores de responsabilidad de GMD</td>
                                <td>
                                    Máximo de tickets Reabiertos en el mes</td>
                                <td>
                                    &gt;=1.5%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                </td>
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
                    <table id="tbCancelados">
                        <thead>
                            <tr>
                                <th colspan="7" align="center">
                                    Tickets Cancelados por duplicidad</th>
                            </tr>
                            <tr>
                                <th style="display:none;"></th>
                                <th>
                                    Detalle</th>
                                <th>
                                    SLA</th>
                                <th>
                                    Total Tickets</th>
                                <th>
                                    Tickets cancelados</th>
                                <th>
                                    Porcentaje</th>
                                <th>
                                    Cumplió</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr id="tr9">
                                <td style="display:none;"></td>
                                <td>
                                    Porcentaje de tickets cancelados por duplicidad</td>
                                <td>
                                    &lt;=0.9%</td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td>
                                </td>
                                <td></td>
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
                <div style="clear: both"></div>
            </div>
            <div id="divLlamadas" class="content">
                <table id="tbIndLlamadas">
                    <thead>
                        <tr>
                            <th>
                                Indicador</th>
                            <th>
                                Criterio de aceptación mensual</th>
                            <th>
                                SLA</th>
                            <th>
                                Total Llamadas</th>
                            <th>
                                Dentro de SLA</th>
                            <th>
                                Porcentaje</th>
                            <th>
                                Cumplió</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                Tiempo para Contestar una llamada Telefónica</td>
                            <td>
                                20 segundos como máximo, luego de finalizar la locución del IVR de bienvenida.</td>
                            <td>&gt;=90%</td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="center">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Tasa de abandono</td>
                            <td>Mide el porcentaje de llamadas que no llegan a ser contestadas por los agentes de la Mesa de Ayuda.</td>
                            <td>&lt;=10%</td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="center">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                Tiempo de Atención del primer nivel.</td>
                            <td>
                                Mide el porcentaje de llamadas atendidas por los agentes del 1er nivel. 25 minutos como máximo.</td>
                            <td>&gt;=98%</td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="right">
                            </td>
                            <td align="center">
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div id="divIngenieria" class="content">
                <div style="float: left; width: 49%;">
                    <table id="tbTRdh">
                        <thead>
                            <tr>
                                <th colspan="6" style="text-align: center">
                                    Tiempo de Respuesta - Dentro de horario de oficina</th>
                            </tr>
                            <tr>
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
                            <tr id="tr4">
                                <td>
                                    Prioridad 0 - 10 minutos</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                            </tr>
                            <tr id="tr6">
                                <td>
                                    Prioridad 1 - 20 minutos</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                            </tr>
                            <tr id="tr7">
                                <td>
                                    Prioridad 2 - 20 minutos</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                            </tr>
                            <tr id="tr8">
                                <td>
                                    Prioridad 3 - 20 minutos</td>
                                <td>
                                    =100%</td>
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
                    <table id="tbTSdh">
                        <thead>
                            <tr>
                                <th colspan="6" style="text-align: center">
                                    Tiempo de Solución - Dentro de horario de oficina</th>
                            </tr>
                            <tr>
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
                            <tr id="tr10">
                                <td>
                                    Prioridad 0 - 1 hora</td>
                                <td>
                                    =100%</td>
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
                                    Prioridad 1 - 2 horas</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                            </tr>
                            <tr id="tr11">
                                <td>
                                    Prioridad 2 - 4 horas</td>
                                <td>
                                    =100%</td>
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
                                    Prioridad 3 - 6 horas</td>
                                <td>
                                    =100%</td>
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
                    <table id="tbTRfh">
                        <thead>
                            <tr>
                                <th colspan="6" style="text-align: center">
                                    Tiempo de Respuesta - Fuera de horario de oficina</th>
                            </tr>
                            <tr>
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
                            <tr id="tr12">
                                <td>
                                    Prioridad 0 - 15 minutos</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                </td>
                            </tr>
                            <tr id="tr13">
                                <td>
                                    Prioridad 1 - 1 hora</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                </td>
                            </tr>
                            <tr id="tr14">
                                <td>
                                    Prioridad 2 - 3 horas</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                            </tr>
                            <tr id="tr15">
                                <td>
                                    Prioridad 3 - 8 horas</td>
                                <td>
                                    =100%</td>
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
                    <table id="tbTSfh">
                        <thead>
                            <tr>
                                <th colspan="6" style="text-align: center">
                                    Tiempo de Solución - Fuera de horario de oficina</th>
                            </tr>
                            <tr>
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
                            <tr id="tr17">
                                <td>
                                    Prioridad 0 - 3 horas</td>
                                <td>
                                    =100%</td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                    </td>
                                <td>
                                </td>
                            </tr>
                            <tr id="tr18">
                                <td>
                                    Prioridad 1- 6 horas</td>
                                <td>
                                    =100%</td>
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
                                    Prioridad 2 - 8 horas</td>
                                <td>
                                    =100%</td>
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
                                    Prioridad 3 - 24 horas</td>
                                <td>
                                    =100%</td>
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
            </div>
            <div id="divTAct" style="float:right;">Tiempo de Actualización: 10 seg</div>
        </div>
    </form>
</body>
</html>
