
using daLib.Exceptions;

namespace daLib.Conventions
{
    public class DayRule : IConvention<string>
    {
        public string dayrule;

        public DayRule(string dayrule)
        {
            this.dayrule = dayrule.ToLower();
        }

        public string getValue()
        {
            return dayrule;
        }

        public bool isValid()
        {
            if (!(dayrule == "mf" || dayrule == "f" || dayrule == "p"))
            {
                return false;
            }
            return true;
        }

        public void Throw() => throw new ExcelException(helperErrorMsg.GenericDayRuleMsg);
    }
}
