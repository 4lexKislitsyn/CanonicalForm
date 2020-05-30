using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core.Interfaces
{
    /// <summary>
    /// Interface represents an object that provides method to render expressions of formula.
    /// </summary>
    public interface IExpressionsRenderer
    {
        /// <summary>
        /// Render expressions of formula.
        /// </summary>
        /// <param name="expressions">Collection of variables.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="expressions"/> is <see langword="null"/>
        /// </exception>
        string Render(IEnumerable<VariablesExpression> expressions);
    }
}
