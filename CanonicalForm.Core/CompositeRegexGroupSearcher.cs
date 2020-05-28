using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CanonicalForm.Core
{
    public class CompositeRegexGroupSearcher : IGroupsSearcher, IFormulaValidator
    {
        private readonly ObjectPool<StringBuilder> _pool;
        private readonly ParenthesisRemover _remover;
        private readonly RegexGroupSearcher _searcher;

        public CompositeRegexGroupSearcher(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
            _remover = new ParenthesisRemover(_pool);
            _searcher = new RegexGroupSearcher();
        }

        public IEnumerable<GroupModel> SearchGroups(string validatedFormula)
        {
            var transformedFormula = _remover.RemoveParenthesis(validatedFormula);
            if (!_searcher.Validate(transformedFormula))
            {
                throw new InvalidFormulaException(validatedFormula);
            }
            return _searcher.SearchGroups(transformedFormula);
        }

        public bool Validate(string formula) => !string.IsNullOrWhiteSpace(formula) && formula.Split('=').Length == 2;
    }
}
