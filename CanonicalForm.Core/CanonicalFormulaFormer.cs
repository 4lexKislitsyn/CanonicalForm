using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CanonicalForm.Core
{
    public class CanonicalFormulaFormer
    {
        private readonly IExpressionSearcher _groupsSearcher;
        private readonly IExpressionsRenderer _renderer;

        public CanonicalFormulaFormer(IExpressionSearcher groupsSearcher, IExpressionsRenderer renderer)
        {
            _groupsSearcher = groupsSearcher ?? throw new ArgumentNullException(nameof(groupsSearcher));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Transform formula to canonical form.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFormulaException"><see cref="IExpressionSearcher"/> cannot form groups collection or render groups that were found.</exception>
        public string Transform(string formula, bool optimize = true)
        {
            if (optimize && _groupsSearcher is IFormulaValidator validator && !validator.Validate(formula))
            {
                // it is faster than catch exception.
                return null;
            }

            var groups = _groupsSearcher.SearchGroups(formula)
                ?? throw new InvalidFormulaException(formula, "cannot find variable's expressions");
            return _renderer.Render(groups) ?? throw new InvalidFormulaException(formula, "cannot render formula");
        }
    }
}
