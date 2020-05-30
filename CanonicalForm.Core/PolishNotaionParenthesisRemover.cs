using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    public class PolishNotaionParenthesisRemover : IParenthesisRemover
    {
        private readonly ObjectPool<StringBuilder> _pool;

        public PolishNotaionParenthesisRemover(ObjectPool<StringBuilder> pool)
        {
            _pool = pool;
        }
        /// <inheritdoc/>
        public string RemoveParenthesis(string formula)
        {
            if (formula is null)
            {
                throw new ArgumentNullException(nameof(formula));
            }

            var polandNotaion = CreatePolandNotaion(formula);
            var result = TransformPolandNotaion(polandNotaion);
            if (result.Count != 1)
            {
                throw new InvalidFormulaException(formula);
            }
            return result.Pop().TrimStart('+');
        }

        private IEnumerable<string> CreatePolandNotaion(string formula)
        {
            var variableBuilder = _pool.Get();
            try
            {
                var polandResult = new List<string>();
                var operatorsStack = new Stack<char>();
                for (int i = 0; i < formula.Length; i++)
                {
                    char tempOperator;

                    void PushVariable()
                    {
                        if (variableBuilder.Length > 0)
                        {
                            polandResult.Add(variableBuilder.ToString());
                            variableBuilder.Clear();
                        }
                    }

                    void PushOperator(char operatorChar)
                    {
                        PushVariable();
                        operatorsStack.Push(operatorChar);
                    }

                    switch (formula[i])
                    {
                        case '-' when i == 0 || formula[i - 1] != '^':
                            PushOperator(formula[i]);
                            break;
                        case '+':
                            var prevOperator = operatorsStack.Peek();
                            if (prevOperator == '-' || prevOperator == '+')
                            {
                                polandResult.Add(operatorsStack.Pop().ToString());
                            }
                            PushOperator(formula[i]);
                            break;
                        case '(':
                            PushOperator(formula[i]);
                            break;
                        case ')':
                            PushVariable();
                            while ((tempOperator = operatorsStack.Pop()) != '(')
                            {
                                polandResult.Add(tempOperator.ToString());
                            }
                            break;
                        default:
                            variableBuilder.Append(formula[i]);
                            break;
                    }
                }
                while (operatorsStack.Count > 0)
                {
                    polandResult.Add(operatorsStack.Pop().ToString());
                }
                return polandResult;
            }
            finally
            {
                _pool.Return(variableBuilder);
            }
        }

        private Stack<string> TransformPolandNotaion(IEnumerable<string> polandNotaionElements)
        {
            var builder = _pool.Get();
            var variables = new Stack<string>();

            try
            {
                foreach (var element in polandNotaionElements)
                {
                    switch (element)
                    {
                        case "-" when variables.Count == 1:
                            variables.Push(ReplacedSignsVariable(builder, variables.Pop()));
                            break;
                        case "+":
                            if (variables.Count > 1)
                            {
                                builder.Append(variables.Pop());
                                builder.Insert(0, element);
                                builder.Insert(0, variables.Pop());
                                variables.Push(builder.ToString());
                                builder.Clear();
                            }
                            break;
                        case "-":
                            if (variables.Count > 1)
                            {
                                builder.Append(ReplacedSignsVariable(builder, variables.Pop()));
                                builder.Insert(0, variables.Pop());
                                variables.Push(builder.ToString());
                                builder.Clear();
                            }
                            break;
                        default:
                            variables.Push(element);
                            break;
                    }
                }
                return variables;
            }
            finally
            {
                _pool.Return(builder);
            }
        }

        static string ReplacedSignsVariable(StringBuilder builder, string variable)
        {
            try
            {
                builder.Append(variable);
                for (var signIndex = 0; signIndex < builder.Length; signIndex++)
                {
                    switch (builder[signIndex])
                    {
                        case '-':
                            builder.Replace('-', '+', signIndex, 1);
                            break;
                        case '+':
                            builder.Replace('+', '-', signIndex, 1);
                            break;
                    }
                }
                if (builder[0] != '-' && builder[0] != '+')
                {
                    builder.Insert(0, '-');
                }
                return builder.ToString();
            }
            finally
            {
                builder.Clear();
            }
        }
    }
}
