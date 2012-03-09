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
/// Summary description for Utilitarios
/// </summary>
public class DataHelper
{

    public static DataTable Distinct(DataTable i_dSourceTable, String[] i_sGroupByColumns, String i_sSortColumn)
    {
        DataView dv = new DataView(i_dSourceTable);
        dv.Sort = i_sSortColumn;
        return dv.ToTable(true, i_sGroupByColumns);
    }

    public static DataTable Filter(DataTable i_dSourceTable, String i_sFilterClause)
    {
        DataView dv = new DataView(i_dSourceTable);
        dv.RowFilter = i_sFilterClause;
        return dv.ToTable();
    }
}

