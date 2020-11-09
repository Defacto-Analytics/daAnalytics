

using daLib.Conventions;
using daLib.Model;
using daAnalyticsExcel.Exposure;
using daAnalyticsExcel.Ribbon.Forms;
using ExcelDna.Integration.CustomUI;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace daAnalyticsExcel.Ribbon
{
    [ComVisible(true)]
    public class ExcelRibbonExposure : ExcelRibbon
    {
        // Manage Models

        public void CalibrateModels(IRibbonControl control1)
        {
            string resultString = "Calibrating Models: \n";

            Index eurois = new daLib.Conventions.Index("EUROIS");
            Index dkkois = new daLib.Conventions.Index("DKKOIS");
            Index eursix = new daLib.Conventions.Index("EUR6M");
            Index dkksix = new daLib.Conventions.Index("DKK6M");


            foreach (KeyValuePair<string, CurveModel> kvp in ExcelFunctionExposure.CurveSet)
            {
                if(kvp.Value.isIndexInModel(eurois))
                {
                    kvp.Value.Calibrate(eurois);
                    resultString += kvp.Key.ToUpper() + " Calibrated with discountindex: EUROIS \n";
                }
                else if(kvp.Value.isIndexInModel(dkkois))
                {
                    kvp.Value.Calibrate(dkkois);
                    resultString += kvp.Key.ToUpper() + " Calibrated with discountindex: DKKOIS \n";
                }
                else if (kvp.Value.isIndexInModel(eursix))
                {
                    kvp.Value.Calibrate(eursix);
                    resultString += kvp.Key.ToUpper() + " Calibrated with discountindex: EUR6M \n";
                }
                else if (kvp.Value.isIndexInModel(dkksix))
                {
                    kvp.Value.Calibrate(dkksix);
                    resultString += kvp.Key.ToUpper() + " Calibrated with discountindex: DKK6M \n";
                }
                else
                {
                    resultString += kvp.Key + " was not calibrated. \n";
                }
            }

            MessageBox.Show(resultString);

        }
        public void RemoveModel(IRibbonControl control)
        {
            ClearRemoveForm form = new ClearRemoveForm("model", "remove");
            form.Show();
        }
        public void CreateModel(IRibbonControl control)
        {
            MessageBox.Show("COMING SOON: CREATE MODEL FORM HERE");
        }

        public void ClearModel(IRibbonControl control)
        {
            ClearRemoveForm form = new ClearRemoveForm("model", "clear");
            form.Show();
        }

        // Manage Portfolio

        public void RemovePortfolio(IRibbonControl control)
        {
            ClearRemoveForm form = new ClearRemoveForm("portfolio", "remove");
            form.Show();
        }
        public void CreatePortfolio(IRibbonControl control)
        {
            MessageBox.Show("COMING SOON: CREATE PORTFOLIO FORM HERE");
        }

        public void ClearPortfolio(IRibbonControl control)
        {
            ClearRemoveForm form = new ClearRemoveForm("portfolio", "clear");
            form.Show();

        }



        // Excel Templates

        public void sqlTemplate(IRibbonControl control)
        {
            openExcelWorkbook("sqltemplate.xlsm");
        }




        private void openExcelWorkbook(string template)
        {
            var spreadsheetLocation = Path.Combine(Directory.GetCurrentDirectory(), template);

            var exApp = new Microsoft.Office.Interop.Excel.Application()
            {
                Visible = true
            };

            Microsoft.Office.Interop.Excel.Workbook exWbk = exApp.Workbooks.Open(spreadsheetLocation, ReadOnly:true);

            //MessageBox.Show(spreadsheetLocation);
        }
    }
}
