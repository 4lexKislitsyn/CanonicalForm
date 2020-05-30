using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IVariableExpressionFactory
    {
        Models.VariablesExpression GetVariable(string variableValue);
    }
}
