

namespace daLib.Conventions
{

/*
 *  Conventions - which are used in this library;
 *  
 *      # Index............Currency&Tenor               => returns string
 *      # Date.............SerialDate&Date&StringDate   => returns DateTime
 *      # DayCount.........DayCount                     => returns string
 *      # DayRule..........DayRule                      => returns string
 *      # IntrumentType....InstrumentType               => returns string
 *      
 * 
 *  -------------------------------------------------------------------------------------------------------------------
 *  
 *  NOTE: Conventions should be used as a parsing layer. Using IConvention classes might be convenient,
 *  but slower performance wise, as we are replacing value types with reference types.
 *  
 *              1. Parse all output into their respective classes
 *              2. Call IsValid()
 *              3. Call Throw() if not valid
 *              4. Extract info from Dictionary<string,IConvention> with GetValue();
 *              
 */


    public interface IConvention
    {
        bool isValid();
        void Throw();
    }

    public interface IConvention<T> : IConvention
    {
        T getValue();
    }

}
