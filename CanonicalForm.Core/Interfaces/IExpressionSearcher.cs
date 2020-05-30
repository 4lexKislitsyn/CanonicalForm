using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IExpressionSearcher
    {
        /// <summary>
        /// Search expressions in formula.
        /// </summary>
        /// <param name="validatedFormula"></param>
        /// <returns></returns>
        IEnumerable<VariablesExpression> SearchGroups(string validatedFormula);
    }
}
