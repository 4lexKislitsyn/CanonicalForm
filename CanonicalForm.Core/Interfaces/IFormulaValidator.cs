using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IFormulaValidator
    {
        bool Validate(string formula);
    }
}
