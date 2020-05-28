using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.ObjectPool;
using CanonicalForm.Core.Models;

namespace CanonicalForm.Core
{
    public class GroupsRenderer : IGroupsRenderer
    {
        private readonly ObjectPool<StringBuilder> _pool;

        public GroupsRenderer(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }

        public string Render(IEnumerable<GroupModel> groups)
        {
            var builder = _pool.Get();
            try
            {
                var aggregatedGroups = groups.GroupBy(x => x.Variable).Select(x => new GroupModel
                {
                    Variable = x.Key,
                    Factor = x.Sum(z => z.Factor),
                    // MaxPower always same for expression
                    MaxPower = x.First().MaxPower
                }).Where(x=> x.Factor != 0).OrderByDescending(x => x.MaxPower).ToArray();
                foreach (var item in aggregatedGroups)
                {
                    builder.Append(item.Factor > 0 ? '+' : '-');
                    if (item.Factor != 1 && item.Factor != -1 || string.IsNullOrEmpty(item.Variable))
                    {
                        builder.Append(item.Factor.ToString("0.##;0.##;0", System.Globalization.CultureInfo.InvariantCulture));
                    }

                    builder.Append(item.Variable);
                }
                if (builder.Length > 1 && builder[0] == '+')
                {
                    builder.Remove(0, 1);
                }
                
                if (builder.Length == 0)
                {
                    builder.Append('0');
                }
                builder.Append("=0");
                return builder.ToString();
            }
            finally
            {
                _pool.Return(builder);
            }
        }
    }
}
