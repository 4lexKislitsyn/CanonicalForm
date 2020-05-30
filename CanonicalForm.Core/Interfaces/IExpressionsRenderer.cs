using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IExpressionsRenderer
    {
        string Render(IEnumerable<VariablesExpression> groups);
    }
}
