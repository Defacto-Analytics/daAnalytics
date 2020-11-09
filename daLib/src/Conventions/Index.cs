

using daLib.Exceptions;


/*
 *  Convention; Index is the whole string --> "EUR3M" or "EUROIS" or "EUR12M"
 *  Convention; Tenor of index --> Tenor("EUR3M") = "3M"
 *  Convention; Currency of index --> Currency("EUR3M") = "EUR"
 *  
 *  Supported index
 *          - IBOR
 *          - OIS
 */



namespace daLib.Conventions
{
    public class Index : IConvention<string>, IDeepClone<Index>
    {

        public string name { get;  set; } // name("EUROIS") => "OIS";
        public string currency { get;  set; } // currency("EUROIS") => "EUR";

        // Does it make sense to include tenor infomation in the Index class? 
        public string tenor { get;  set; } // Cant deduce this from index name alone --> set this later when we have more info.

        public Index(string currency, string index_name)
        {
            initIndex(currency, index_name);
        }

        public Index(string index)
        {
            initIndex(index.Substring(0, 3), index.Substring(3));
        }

        public void setTenor(string tenor)
        {
            this.tenor = tenor.ToLower();
        }
        public string getTenor()
        {
            return this.tenor;
        }
        public void CheckForTenor()
        {
            if (!(this.tenor != null && ValidDate.isTenorDate(this.tenor)))
            {
                throw new ExcelException("Wrongly formatted tenor given to index");
            }
        }

        private void initIndex(string currency, string index_name)
        {
            this.currency = currency.ToLower();
            this.name = index_name.ToLower();
        }


        #region IConvention<string>
        public string getValue()
        {
            return this.currency + this.name; // careful changing this -> acts as the key in CurveModel.ForwardCurves;
        }

        public bool isValid()
        {

            if (!(currency == "eur" || currency == "dkk"))
            {
                return false;
            }

            if (!(name == "ois" || name == "1m" || name == "3m" || name == "6m" || name == "12m"))
            {
                return false;
            }

            return true;
        }

        public void Throw() => throw new ExcelException(helperErrorMsg.GenericIndexMsg);

        #endregion
        #region IDeepClone<Index>
        public Index DeepClone()
        {
            return this.MemberwiseClone() as Index;
        }

        object IDeepClone.DeepClone()
        {
            return this.DeepClone();
        }
        #endregion

        public bool Equals(Index o)
        {
            if (name == o.name && this.tenor == o.tenor && this.currency == o.currency)
            {
                return true;
            }
            return false;
        }

    }
}
