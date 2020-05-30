using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.ObjectPool;
using CanonicalForm.Core.Models;

namespace CanonicalForm.Core
{
    /// <summary>
    /// Class that provides method to group and render expressions of formula.
    /// </summary>
    public class GroupsRenderer : IExpressionsRenderer
    {
        private readonly ObjectPool<StringBuilder> _pool;

        /// <summary>
        /// Create an instance of <see cref="GroupsRenderer"/>.
        /// </summary>
        /// <param name="pool"></param>
        public GroupsRenderer(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }

        /// <inheritdoc/>
        public string Render(IEnumerable<VariablesExpression> expressions)
        {
            if (expressions is null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }
            var builder = _pool.Get();
            try
            {
                var aggregatedGroups = expressions.GroupBy(x => x.Variable).Select(x => new VariablesExpression
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
