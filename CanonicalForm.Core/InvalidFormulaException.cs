using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    public class InvalidFormulaException : Exception
    {
        public InvalidFormulaException(string formula) : base($"Formula '{formula ?? "null"}' is invalid formula")
        {
            Formula = formula;
        }

        public string Formula { get; }
    }
}
