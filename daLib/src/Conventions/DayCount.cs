




using daLib.Exceptions;

namespace daLib.Conventions
{
    public class DayCount : IConvention<string>
    {
        public string daycount;

        public DayCount(string daycount)
        {
            this.daycount = daycount;
        }
        public string getValue()
        {
            return daycount;
        }

        public bool isValid()
        {
            if (!(daycount == "act/360" || daycount == "act/365" || daycount == "act/365.25" || daycount == "30/360"))
            {
                return false;
            }
            return true;
        }

        public void Throw() => throw new ExcelException(helperErrorMsg.GenericDayCountMsg);
    }
}



