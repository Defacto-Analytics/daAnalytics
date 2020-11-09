
using System.Collections.Generic;

using daLib.Model;
using daLib.Exceptions;
using daLib.Portfolios;

using ExcelDna.Integration;
using ExcelDna.IntelliSense;


namespace daAnalyticsExcel.Exposure
{
    public partial class ExcelFunctionExposure : IExcelAddIn
    {
        public const string prefix = "da";
        public static Dictionary<string,CurveModel> CurveSet;
        public static Dictionary<string, Portfolio> PortfolioSet;

        public static CurveModel TryGetCurveModel(string CurveModel_ID)
        {
            
            string tmpCurveModel_ID = CurveModel_ID.ToLower();
            try
            {
                return CurveSet[tmpCurveModel_ID];
            }
            catch
            {
                throw new ExcelException(helperErrorMsg.CurveModel_CantFindModel(CurveModel_ID));
            }
        }
        public static Portfolio TryGetPortfolioSet(string Portfolio_ID)
        {
            string tmpPortfolio_ID = Portfolio_ID.ToLower();
            try
            {
                return PortfolioSet[tmpPortfolio_ID];
            }
            catch
            {
                throw new ExcelException(helperErrorMsg.Portfolio_CantFindPortfolio(Portfolio_ID));
            }
        }

        public void AutoOpen()
        {
            CurveSet = new Dictionary<string, CurveModel>();
            PortfolioSet = new Dictionary<string, Portfolio>();
            
            
            IntelliSenseServer.Install();
            ExcelIntegration.RegisterUnhandledExceptionHandler(
                delegate (object ex)
                {
                    return "ERROR: " + ex.ToString();
                }
            );
            
        
        }

        public void AutoClose()
        {
            IntelliSenseServer.Uninstall();
        }
    }
}

