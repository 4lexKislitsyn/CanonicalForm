using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Models
{
    public class VariablesExpression
    {
        public VariablesExpression(GroupModel groupedVariable)
        {

        }

        public int Factor { get; set; }
        public int MaxPower { get; set; }
    }
}
