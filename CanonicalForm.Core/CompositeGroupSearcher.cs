using CanonicalForm.Core.Interfaces;
using CanonicalForm.Core.Models;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CanonicalForm.Core
{
    public class CompositeGroupSearcher : IExpressionSearcher, IFormulaValidator
    {
        private readonly IParenthesisRemover _remover;
        private readonly IExpressionSearcher _searcher;

        /// <summary>
        /// Create an instance of class <see cref="CompositeGroupSearcher"/>.
        /// </summary>
        /// <param name="remover"></param>
        /// <param name="searcher"></param>
        public CompositeGroupSearcher(IParenthesisRemover remover, IExpressionSearcher searcher)
        {
            _remover = remover;
            _searcher = searcher;
        }
        /// <inheritdoc/>
        public IEnumerable<VariablesExpression> SearchGroups(string validatedFormula)
        {
            if (!Validate(validatedFormula))
            {
                throw new InvalidFormulaException(validatedFormula);
            }
            var transformedFormula = string.Join("=", validatedFormula.Split('=').Select(_remover.RemoveParenthesis));
            if ((_searcher as IFormulaValidator)?.Validate(transformedFormula) == false)
            {
                throw new InvalidFormulaException(validatedFormula);
            }
            return _searcher.SearchGroups(transformedFormula);
        }
        /// <inheritdoc/>
        public bool Validate(string formula) => !string.IsNullOrWhiteSpace(formula) 
            && formula.Split('=').Length == 2 
            && formula.Count(x=> x =='(') == formula.Count(x => x == ')');
    }
}
