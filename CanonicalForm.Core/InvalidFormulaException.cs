using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    public class InvalidFormulaException : Exception
    {
        public InvalidFormulaException(string formula)
            : base($"Formula is invalid.")
        {
            Formula = formula;
        }
        public InvalidFormulaException(string formula, string reason)
            : base($"Formula is invalid : {reason}.")
        {
            Formula = formula;
        }

        public string Formula { get; }
    }
}
