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

    public static DataTable GroupBy(DataTable i_dSourceTable, String[] i_sGroupByColumns, string i_sComputeColumn, string i_sComputeFunction)
    {
        string sComputeOperation = string.Format("{0}({1})", i_sComputeFunction, i_sComputeColumn); 
        string sComputeCondition = null;

        DataView dv = new DataView(i_dSourceTable);
        DataTable dtGroup = dv.ToTable(true, i_sGroupByColumns);
        dtGroup.Columns.Add(new DataColumn("TOTAL", typeof(int)));
        
        foreach (DataRow dr in dtGroup.Rows)
        {
            sComputeCondition = getFilterCondition(i_sGroupByColumns, dr);
            dr["TOTAL"] = i_dSourceTable.Compute(sComputeOperation, sComputeCondition);
        }

        return dtGroup;
    }

    private static string getFilterCondition(String[] i_sGroupByColumns, DataRow drToCompute)
    {
        string sCondition = null;
        for (int i = 0; i < i_sGroupByColumns.Length; i++)
        {
            if (typeof(string) == drToCompute[i_sGroupByColumns[i]].GetType())
                sCondition += string.Format("{0} = '{1}' and ", i_sGroupByColumns[i], drToCompute[i_sGroupByColumns[i]]);
            else
                sCondition += string.Format("{0} = {1} and ", i_sGroupByColumns[i], drToCompute[i_sGroupByColumns[i]]);
        }
        if (sCondition.EndsWith("and ")) sCondition = sCondition.Substring(0, sCondition.Length - 5);
        return sCondition;
    }

    
}

