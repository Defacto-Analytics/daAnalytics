using daLib.Conventions;
using daLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace daLib.Currencies
{   
    public class Currency : IConvention<string>
    {
        public string name;

        public Currency(string currency)
        {
            name = currency.ToLower();
        }

        public string getValue()
        {
            return name;
        }
        public bool isValid()
        {
            if (name == "eur" || name == "dkk")
            {
                return true;
            }
            return false;
        }
        public void Throw()
        {
            throw new ExcelException($"The currency \"{name}\" is not supported");
        }
    }
}
