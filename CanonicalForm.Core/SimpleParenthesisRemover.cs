using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    public class SimpleParenthesisRemover: IParenthesisRemover
    {
        private readonly ObjectPool<StringBuilder> _pool;

        public SimpleParenthesisRemover(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }

        public string RemoveParenthesis(string formula)
        {
            var stack = new Stack<int>();
            var invert = false;
            var builder = _pool.Get();
            builder.Append(formula);
            for (var i = 0; i < builder.Length; i++)
            {
                var character = builder[i];
                switch (character)
                {
                    case '(':
                        if (i > 0 && builder[i - 1] == '-')
                        {
                            invert = !invert;
                            stack.Push(-1);
                        }
                        else
                        {
                            stack.Push(1);
                        }
                        builder.Remove(i--, 1);
                        break;
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
                        break;
                    case '+' when invert:
                        builder.Replace('+', '-', i, 1);
                        break;
                    case ' ':
                        builder.Remove(i--, 1);
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
