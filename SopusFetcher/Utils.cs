using ClosedXML.Excel;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace SopusFetcher
{
    public class Utils
    {
        public static DataTable GetDataTableFromExcel(string filePath, string tab)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (XLWorkbook workBook = new XLWorkbook(fs))
            {
                IXLWorksheet workSheet = workBook.Worksheet(tab);

                DataTable dt = new DataTable();

                bool firstRow = true;
                foreach (IXLRow row in workSheet.Rows())
                {
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        dt.Rows.Add();
                        int i = 0;

                        try
                        {
                            foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                i++;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            dt.Rows.RemoveAt(dt.Rows.Count - 1);
                            break;
                        }
                    }
                }
                return dt;
            }
        }
    }
}
