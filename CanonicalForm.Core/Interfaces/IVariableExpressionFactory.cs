using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IVariableExpressionFactory
    {
        /// <summary>
        /// Parse variable.
        /// </summary>
        /// <param name="variableValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Cannot parse passed expression.</exception>
        Models.VariablesExpression GetVariable(string variableValue);
    }
}
