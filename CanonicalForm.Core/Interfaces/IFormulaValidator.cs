﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IFormulaValidator
    {
        /// <summary>
        /// Check is formula valid.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        bool Validate(string formula);
    }
}
