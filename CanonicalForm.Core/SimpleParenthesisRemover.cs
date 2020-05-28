using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    /// <summary>
    /// Class for opening parenthesis in a formula.
    /// </summary>
    public class SimpleParenthesisRemover: IParenthesisRemover
    {
        private readonly ObjectPool<StringBuilder> _pool;
        /// <summary>
        /// Create an instance of class <see cref="SimpleParenthesisRemover"/>.
        /// </summary>
        /// <param name="pool"></param>
        public SimpleParenthesisRemover(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }

        /// <inheritdoc/>
        public string RemoveParenthesis(string formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(nameof(formula));
            }
            var stack = new Stack<int>();
            var invert = false;
            var builder = _pool.Get();
            builder.Append(formula);
            var justOpened = false;
            for (var i = 0; i < builder.Length; i++)
            {
                var character = builder[i];
                switch (character)
                {
                    case '(':
                        if (i > 0 && builder[i - 1] == '-')
                        {
                            if (!justOpened)
                            {
                                invert = !invert;
                            }
                            stack.Push(-1);
                        }
                        else
                        {
                            stack.Push(1);
                        }
                        builder.Remove(i--, 1);
                        justOpened = true;
                        continue;
                    case ')':
                        var topSign = stack.Pop();
                        if (topSign < 0)
                        {
                            invert = !invert;
                        }
                        builder.Remove(i--, 1);
                        break;
                    case '-' when invert:
                        builder.Replace('-', '+', i, 1);
                        if (i > 0 && (builder[i - 1] == '-' || builder[i - 1] == '+'))
                        {
                            builder.Remove(--i, 1);
                        }
                        break;
                    case '+' when invert:
                        builder.Replace('+', '-', i, 1);
                        if (i > 0 && (builder[i - 1] == '-' || builder[i - 1] == '+'))
                        {
                            builder.Remove(--i, 1);
                        }
                        break;
                    case ' ':
                        builder.Remove(i--, 1);
                        break;
                }
                justOpened = false;
            }
            if (builder.Length>0 && builder[0] == '+')
            {
                builder.Remove(0, 1);
            }
            return builder.ToString();
        }
    }
}
