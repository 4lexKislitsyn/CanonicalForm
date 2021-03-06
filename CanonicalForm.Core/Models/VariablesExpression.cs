﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CanonicalForm.Core.Models
{
    /// <summary>
    /// Class that keeps information about variables expressions, e.g. '-5x^2y'.
    /// </summary>
    [DebuggerDisplay("{Factor}{Variable,nq} (max power = {MaxPower})")]
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
