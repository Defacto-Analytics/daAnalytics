using daLib.Conventions;
using daLib.Model;
using dskAnalyticsExcel.Exposure;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dskAnalyticsExcel
{
    public partial class ExcelFunctionExposure : IExcelAddIn
    {
        public static void CalibrateModels()
        {
            string resultString = "Calibrating Models: \n";

            Index eurois = new daLib.Conventions.Index("EUROIS");
            Index dkkois = new daLib.Conventions.Index("DKKOIS");
            Index eursix = new daLib.Conventions.Index("EUR6M");
            Index dkksix = new daLib.Conventions.Index("DKK6M");


            foreach (KeyValuePair<string, CurveModel> kvp in CurveSet)
            {
                if (kvp.Value.isIndexInModel(eurois))
                {
                    kvp.Value.Calibrate(eurois);
                    resultString += kvp.Key.ToUpper() + " Calibrated with discountindex: EUROIS \n";
                }
                else if (kvp.Value.isIndexInModel(dkkois))
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

    }
}
