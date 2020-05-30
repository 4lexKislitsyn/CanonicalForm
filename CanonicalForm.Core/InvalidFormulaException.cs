using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    public class InvalidFormulaException : Exception
    {
        public InvalidFormulaException(string formula)
            : base($"Formula '{formula ?? "null"}' is invalid formula.")
        {
            Formula = formula;
        }
        public InvalidFormulaException(string formula, string reason)
            : base($"Formula '{formula ?? "null"}' is invalid formula : {reason}.")
        {
            Formula = formula;
        }

        public string Formula { get; }
    }
}
