using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    public interface IParenthesisRemover
    {
        /// <summary>
        /// Remove parenthesis from formula.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        string RemoveParenthesis(string formula);
    }
}
