using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string Transform(string formula)
        {
            formula = formula.Trim();
            if (_groupsSearcher is IFormulaValidator validator && !validator.Validate(formula))
            {
                return null;
            }

            var groups = _groupsSearcher.SearchGroups(formula);
            return _renderer.Render(groups) ?? "Cannot render formula";
        }
    }
}
