using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTool
{
    //  User defined Exception that wraps all Errors and 
    //  Exceptions that occur during the Execution Process
    // Author : Smitha Madhavamurthy 
    // Date : 04-25-2007
    class ATLogicException :Exception
    {
        String errorMessage = null;
        

        public ATLogicException(String message)
        {
            this.errorMessage = message;
        }

        


        /**
         * returns the error message
         */
        public string getATLogicErrorMessage()
        {
            return errorMessage;
           
        }

    }
}
