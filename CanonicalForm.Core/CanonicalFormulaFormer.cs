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
        private readonly IGroupsSearcher _groupsSearcher;
        private readonly IGroupsDictionaryBuilder _builder;
        private readonly IGroupsRenderer _renderer;

        public CanonicalFormulaFormer(IGroupsSearcher groupsSearcher, 
            IGroupsDictionaryBuilder builder, IGroupsRenderer renderer)
        {
            _groupsSearcher = groupsSearcher ?? throw new ArgumentNullException(nameof(groupsSearcher));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public string Transform(string formula)
        {
            formula = formula.Trim();
            if (_groupsSearcher is IFormulaValidator validator && !validator.Validate(formula))
            {
                return null;
            }

            var groupsDictionary = _builder.Build(_groupsSearcher.SearchGroups(formula));
            return _renderer.Render(groupsDictionary) ?? "Cannot render formula";
        }
    }
}
