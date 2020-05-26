using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanonicalForm.Core
{
    public class GroupsDictionaryBuilder : IGroupsDictionaryBuilder
    {
        public IDictionary<string, GroupModel> Build(IEnumerable<GroupModel> groups)
        {
            return groups.GroupBy(x => x.Variable).ToDictionary(x => x.Key, x => new GroupModel
            {
                Variable = x.Key,
                Factor = x.Sum(z=> z.Factor),
                Power = x.First().Power
            });
        }
    }
}
