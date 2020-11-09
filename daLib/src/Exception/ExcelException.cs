
namespace daLib.Exceptions
{
    public class ExcelException : System.Exception
    { 
        public ExcelException(string message) : base(message) { }
        public override string ToString()
        {
            return this.Message;
        }
    }
    public static class helperErrorMsg
    {
        
        // ############# Dayrule #############
        public static readonly string GenericDayRuleMsg = "Input for dayrule is not valid.";

        // ############# Daycount #############
        public static readonly string GenericDayCountMsg = "Input for daycount is not valid.";

        // ############# Calendar #############
        public static readonly string GenericCalendarMsg = "Input for businesscalendar is not valid.";

        // ############# Index #############
        public static readonly string GenericIndexMsg = "Input for index is not valid.";

        // ############# Tenor #############
        public static readonly string WrongFormatTenor = "Wrongly formatted tenor given to index"; 

        // ############# CurveModel #############
        public static string CurveModel_CantFindModel(string model) => $"Could not find model \"{model}\"";

        // ############# Portfolio #############
        public static string Portfolio_CantFindPortfolio(string portfolio) => $"Could not find portfolio \"{portfolio}\"";





    }





}
