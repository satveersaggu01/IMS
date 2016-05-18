using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using DocumentFormat.OpenXml.Extensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
namespace DataAccessLayer
{
    /// <summary>
    /// It contains method to download a excel file using Open XML
    /// </summary>
    public static class ExcelDocument
    {
        /// <summary>
        /// Create the exel document for streaming.
        /// </summary>
        /// <param name="documentName">Excel file name.</param>
        /// <returns>Memory stream.</returns>
        public static MemoryStream Create(DataSet dataSet)
        {
            return CreateSpreadSheet(dataSet);
        }

        /// <summary>
        /// Create the spreadsheet.
        /// </summary>
        /// <param name="documentName">Excel file name.</param>
        /// <returns>Memory stream.</returns>
        private static MemoryStream CreateSpreadSheet(DataSet ds)
        {
            MemoryStream stream = SpreadsheetReader.Create();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, true))
            {
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    DataTable dataTable = RemoveColumnInDataTable(ds.Tables[i]);
                    WorksheetPart newWorkSheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                    newWorkSheetPart.Worksheet = new Worksheet(new SheetData());
                    newWorkSheetPart.Worksheet.Save();
                    CreateNewSheet(spreadSheet, ref newWorkSheetPart, dataTable.TableName);

                    WorksheetWriter writer = new WorksheetWriter(spreadSheet, newWorkSheetPart);
                    SpreadsheetStyle style = SpreadsheetReader.GetDefaultStyle(spreadSheet);
                    style.SetBorder("000000", BorderStyleValues.Thin);
                    style.IsBold = true;
                    CreateSheetHeader(spreadSheet, ref writer, ref style, dataTable);
                    style.IsBold = false;
                    writer.PasteDataTable(dataTable, "A2", style);
                    spreadSheet.WorkbookPart.Workbook.Save();
                }
                //Remove first 3 default tabs (Sheet1 ~ Sheet3) 
                spreadSheet.WorkbookPart.Workbook.Sheets.FirstChild.Remove();
                spreadSheet.WorkbookPart.Workbook.Sheets.FirstChild.Remove();
                spreadSheet.WorkbookPart.Workbook.Sheets.FirstChild.Remove();
            }

            return stream;
        }

        /// <summary>
        /// Sheet's Header.
        /// </summary>
        /// <param name="spreadSheet">SpreadDocument</param>
        /// <param name="writer">WorksheetWriter</param>
        /// <param name="style">SpreadsheetStyle</param>
        /// <param name="dataTable">DataTable</param>
        private static void CreateSheetHeader(SpreadsheetDocument spreadSheet, ref WorksheetWriter writer, ref SpreadsheetStyle style, DataTable dataTable)
        {
            for (int x = 0; x < dataTable.Columns.Count; x++)
            {
                string columnName = GetExcelColumnValue(x + 1);
                writer.PasteText(columnName + "1",
                    RenameColumn(dataTable.TableName, dataTable.Columns[x].ColumnName), style);
            }
        }

        /// <summary>
        /// Excel Column Value.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        private static string GetExcelColumnValue(int columnNumber)
        {
            if (columnNumber <= 26)
            {
                return ((char)(columnNumber + 64)).ToString();
            }
            columnNumber--;
            return GetExcelColumnValue(columnNumber / 26) +
                GetExcelColumnValue((columnNumber % 26) + 1);
        }

        /// <summary>
        /// Create New Sheet.
        /// </summary>
        /// <param name="spreadSheet"></param>
        /// <param name="newWorksheetPart"></param>
        /// <param name="sheetName"></param>
        private static void CreateNewSheet(SpreadsheetDocument spreadSheet, ref WorksheetPart newWorksheetPart, string sheetName)
        {
            Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);
            // Get a unique ID for the new worksheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);
        }

        private static string RenameColumn(string TableName, string ColumnName)
        {
            string rename = string.Empty;
            switch (TableName + " " + ColumnName)
            {
                case "PercentUseByRole % of Total":
                case "DistinctVisitorsByJobTitle % of Total":
                    rename = "Percentage of Total";
                    break;
                case "DistinctVisitorsByJobTitle Role":
                    rename = "Job Title";
                    break;
                case "NoClickSearches rm_user_name":
                case "ResultsPagedMoreThanTen rm_user_name":
                case "ZeroResultsReturned Name":
                    rename = "User Name";
                    break;
                case "NoClickSearches rm_mainquery":
                case "ResultsPagedMoreThanTen rm_mainquery":
                    rename = "Search Query";
                    break;
                case "NoClickSearches rm_selectedserver":
                case "ResultsPagedMoreThanTen rm_selectedserver":
                case "FirmFilterUsage Tab":
                    rename = "Tab Type";
                    break;
                case "ResultsPagedMoreThanTen rm_page":
                    rename = "Result Page";
                    break;
                case "TopSearches Most popular search terms":
                    rename = "Top Search Terms";
                    break;
                case "FirmFilterUsage Filter/Sort Name":
                    rename = "Filter Used";
                    break;
                case "FirmFilterUsage Filter/Sort Total Use":
                    rename = "Filer Use";
                    break;
                case "FirmFilterUsage Percent Used":
                    rename = "Percentage of Filter Use in All Searches";
                    break;
                default:
                    rename = ColumnName;
                    break;
            }

            return rename;
        }

        private static DataTable RemoveColumnInDataTable(DataTable dataTable)
        {
            string tableName = dataTable.TableName;
            List<DataColumn> dataColList = new List<DataColumn>();
            foreach (DataColumn dc in dataTable.Columns)
            {
                if (dc.ColumnName == "rm_session_id" || dc.ColumnName == "rm_session_id_click")
                    dataColList.Add(dc);
            }

            if (dataColList.Count > 0)
            {
                dataColList.ForEach(delegate(DataColumn objDC)
                {
                    dataTable.Columns.Remove(objDC);
                });
            }

            return dataTable;
        }
    }

}
