using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.ObjectPool;

namespace CanonicalForm.Core
{
    public class GroupsDictionaryRenderer : IGroupsRenderer
    {
        private readonly ObjectPool<StringBuilder> _pool;

        public GroupsDictionaryRenderer(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }

        public string Render(IDictionary<string, GroupModel> groups)
        {
            var builder = _pool.Get();
            try
            {
                foreach (var item in groups.Values.OrderByDescending(x => x.Power))
                {
                    if (item.Factor == 0)
                    {
                        continue;
                    }

                    if (item.Factor > 0)
                    {
                        builder.Append('+');
                    }
                    else
                    {
                        builder.Append('-');
                    }
                    builder.Append(' ');

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
