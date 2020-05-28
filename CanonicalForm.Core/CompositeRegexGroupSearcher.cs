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
            validatedFormula = _remover.RemoveParenthesis(validatedFormula);
            if (!_searcher.Validate(validatedFormula))
            {
                throw new Exception("Invalid formula");
            }
            return _searcher.SearchGroups(validatedFormula);
        }

        public bool Validate(string formula) => _searcher.Validate(formula);
    }
}
