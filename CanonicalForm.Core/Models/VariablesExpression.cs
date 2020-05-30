using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Models
{
    public class VariablesExpression
    {
        /// <summary>
        /// Max power of the inner variables.
        /// </summary>
        public int MaxPower { get; set; }
        /// <summary>
        /// Expression of the variable.
        /// </summary>
        public string Variable { get; set; }
        /// <summary>
        /// Factor of the expression.
        /// </summary>
        public double Factor { get; set; }
    }
}
