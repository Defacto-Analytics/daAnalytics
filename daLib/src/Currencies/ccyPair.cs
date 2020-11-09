using daLib.Conventions;
using daLib.Exceptions;

namespace daLib.Currencies
{
    public enum ccyComparison
    {
        CompletlyEqual,
        SwichedEqual,
        NotEqual
    }


    public class ccyPair : IConvention
    {
        // notation     
        //              dom => domestic
        //              for => foreign
        //
        // example
        //              eur/dkk => dom/for
        //
        // questions
        //
        //              
        // 


        public double? value;
        public Currency domCurrency;
        public Currency forCurrency;

        public ccyPair(Currency domestic, Currency foreign, double? value)
        {
            this.value = value;
            this.domCurrency = domestic;
            this.forCurrency = foreign;
        }

        public ccyComparison ccyCompare(ccyPair pair)
        {
            if ((pair.domCurrency.getValue() == this.domCurrency.getValue() && pair.forCurrency.getValue() == this.forCurrency.getValue()))
            {
                return ccyComparison.CompletlyEqual;
            }
            else if (pair.domCurrency.getValue() == this.domCurrency.getValue() || pair.forCurrency.getValue() == this.forCurrency.getValue())
            {
                return ccyComparison.SwichedEqual;
            }
            else
            {
                return ccyComparison.NotEqual;
            }
        }

        public bool HasValue()
        {
            return value.HasValue;
        }

        public double CcyInDomestic()
        {
            return value.Value;
        }

        public double CcyInForeign()
        {
            return 1.0 / value.Value;
        }

        public bool isValid()
        {
            return domCurrency.isValid() && forCurrency.isValid() ? true : false;
        }

        public void Throw()
        {
            throw new ExcelException($"Currency pair \"{domCurrency.getValue()}\\{forCurrency.getValue()}\" is not valid");
        }
    }
}
