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
                    builder.Append(item.Factor > 0 ? '+' : '-').Append(' ');
                    if (item.Factor != 1 && item.Factor != -1)
                    {
                        builder.Append(item.Factor.ToString("0.##;0.##;0", System.Globalization.CultureInfo.InvariantCulture));
                    }

                    builder.Append(item.Variable);
                    builder.Append(' ');
                }
                if (builder.Length > 2 && builder[0] == '+')
                {
                    builder.Remove(0, builder[1] == ' ' ? 2 : 1);
                }
                builder.Append("= 0");
                return builder.ToString();
            }
            finally
            {
                _pool.Return(builder);
            }
        }
    }
}
