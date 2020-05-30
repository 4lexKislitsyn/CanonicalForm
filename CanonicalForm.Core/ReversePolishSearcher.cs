using CanonicalForm.Core.Interfaces;
using CanonicalForm.Core.Models;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CanonicalForm.Core
{
    public class ReversePolishSearcher : IExpressionSearcher, IFormulaValidator
    {
        private readonly ObjectPool<StringBuilder> _pool;
        private readonly IVariableExpressionFactory _expressionFactory;

        public ReversePolishSearcher(ObjectPool<StringBuilder> pool, IVariableExpressionFactory expressionFactory)
        {
            _pool = pool;
            _expressionFactory = expressionFactory;
        }

        public IEnumerable<VariablesExpression> SearchGroups(string validatedFormula)
        {
            if (string.IsNullOrWhiteSpace(validatedFormula))
            {
                throw new InvalidFormulaException(validatedFormula, "formula is null or whitespace");
            }

            if (validatedFormula.IndexOf('=') < 0)
            {
                throw new InvalidFormulaException(validatedFormula, "must contain '=' sign");
            }

            var state = new SearchState(_pool.Get(), _expressionFactory, validatedFormula);
            var formulaBuilder = _pool.Get().Append(validatedFormula);

            try
            {
                for (int i = 0; i < formulaBuilder.Length; i++)
                {
                    if (formulaBuilder[i] == ' ')
                    {
                        formulaBuilder.Remove(i--, 1);
                        continue;
                    }
                    var currentChar = formulaBuilder[i];

                    if (currentChar == '=')
                    {
                        // (x=
                        var hasOpenedGroup = state.OperatorsStack.Contains('(');
                        // x=x=
                        var hasAnotherEqualSign = state.OperatorsStack.Contains('=');
                        // =x
                        var noLeftSideVariables = state.VariableBuilder.Length == 0 && state.ExpressionsStack.Count == 0;
                        // x+=; length is important : x= (builder == x, operators stack is empty)
                        var hasOpenedOperator = state.VariableBuilder.Length == 0 && state.ExpressionsStack.Count > 0;
                        if (hasOpenedGroup || hasAnotherEqualSign || noLeftSideVariables)
                        {
                            throw new InvalidFormulaException(validatedFormula, "'=' operator should be single and follow ')' or variable");
                        }
                    }

                    switch (currentChar)
                    {
                        case '=':
                        case '-' when i == 0 || formulaBuilder[i - 1] != '^':
                        case '+':
                            {
                                state.CheckAndAddNilElement(currentChar);
                                if (state.OperatorsStack.Count > 0)
                                {
                                    var prevOperator = state.OperatorsStack.Peek();
                                    if (prevOperator == '-' || prevOperator == '+')
                                    {
                                        state.ApplyTopOperator();
                                    }
                                }
                                state.PushOperator(currentChar, i);
                            }
                            break;
                        case '(':
                            state.PushOperator(currentChar, i);
                            break;
                        case ')':
                            state.ApplyParenthesis();
                            break;
                        default:
                            state.VariableBuilder.Append(currentChar);
                            break;
                    }
                }
                while (state.OperatorsStack.Count > 0)
                {
                    state.ApplyTopOperator();
                }
                if (state.ExpressionsStack.Count > 1)
                {
                    throw new InvalidFormulaException(validatedFormula, "cannot transform formula to single expression");
                }
                return state.ExpressionsStack.Pop().Expressions;
            }
            finally
            {
                _pool.Return(state.VariableBuilder);
                _pool.Return(formulaBuilder);
            }
        }
        /// <inheritdoc/>
        public bool Validate(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                return false;
            }
            var split = formula.Split('=');
            return split.Length == 2 && split.All(IsValidPart);
        }

        private static bool IsValidPart(string formulaPart)
        {
            return !string.IsNullOrWhiteSpace(formulaPart)
                && formulaPart[0] != ')'
                && formulaPart[formulaPart.Length - 1] != '+'
                && formulaPart[formulaPart.Length - 1] != '-'
                && formulaPart[formulaPart.Length - 1] != '(';
        }


        private static bool IsSeparateGroupsOperator(char op) => op == '(' || op == '=';

        /// <summary>
        /// Grouping of multiple variables to apply sign changes.
        /// </summary>
        private class VariableExpressionsGroup
        {
            private readonly List<VariablesExpression> _expressions = new List<VariablesExpression>();
            public VariableExpressionsGroup(VariablesExpression expression)
            {
                _expressions.Add(expression);
            }

            internal IEnumerable<VariablesExpression> Expressions => _expressions.Where(x => x.Factor != 0);

            internal void Combine(VariableExpressionsGroup group, bool changeSign)
            {
                if (changeSign)
                {
                    group.ChangeSign();
                }
                _expressions.AddRange(group._expressions);
                group._expressions.Clear();
                _expressions.RemoveAll(x => x.Factor == 0);
            }

            internal void ChangeSign() => _expressions.ForEach(x => x.Factor *= -1);
        }

        /// <summary>
        /// State of search variable expressions.
        /// </summary>
        private class SearchState
        {
            private readonly IVariableExpressionFactory _expressionFactory;
            private readonly string _formula;
            /// <summary>
            /// Create an instance of <see cref="SearchState"/>.
            /// </summary>
            /// <param name="variableBuidler"></param>
            /// <param name="factory"></param>
            /// <param name="formula"></param>
            internal SearchState(StringBuilder variableBuidler, IVariableExpressionFactory factory, string formula)
            {
                VariableBuilder = variableBuidler;
                _expressionFactory = factory;
                _formula = formula;
            }
            /// <summary>
            /// Builder for variable.
            /// </summary>
            internal StringBuilder VariableBuilder { get; }
            /// <summary>
            /// Stack of found expressions.
            /// </summary>
            internal Stack<VariableExpressionsGroup> ExpressionsStack { get; } = new Stack<VariableExpressionsGroup>();
            /// <summary>
            /// Operators to apply.
            /// </summary>
            internal Stack<char> OperatorsStack { get; } = new Stack<char>();

            /// <summary>
            /// Push variable from <see cref="VariableBuilder"/> to <see cref="ExpressionsStack"/>.
            /// </summary>
            internal void PushVariable()
            {
                if (VariableBuilder.Length <= 0)
                {
                    return;
                }

                var variablevalue = VariableBuilder.ToString();
                var variable = _expressionFactory.GetVariable(variablevalue);
                if (variable == null)
                {
                    throw new InvalidFormulaException(_formula, $"cannot parse variable '{variablevalue}'");
                }
                ExpressionsStack.Push(new VariableExpressionsGroup(variable));
                VariableBuilder.Clear();
            }
            /// <summary>
            /// Push operator to <see cref="OperatorsStack"/>.
            /// </summary>
            /// <param name="operatorChar"></param>
            /// <param name="position"></param>
            internal void PushOperator(char operatorChar, int position)
            {
                if (!IsSeparateGroupsOperator(operatorChar))
                {
                    if (OperatorsStack.Count != 0 && VariableBuilder.Length == 0 && !IsSeparateGroupsOperator(OperatorsStack.Peek()))
                    {
                        // -- +- 
                        throw new InvalidFormulaException(_formula, $"operator at position {position} should follow variable");
                    }
                }
                PushVariable();
                OperatorsStack.Push(operatorChar);
            }
            /// <summary>
            /// Apply operator from <see cref="OperatorsStack"/>.
            /// </summary>
            internal void ApplyTopOperator()
            {
                if (OperatorsStack.Count == 0)
                {
                    return;
                }
                var operatorChar = OperatorsStack.Pop();
                if (operatorChar == '(')
                {
                    throw new InvalidFormulaException(_formula, "all parentheses must be closed");
                }
                PushVariable();
                if (ExpressionsStack.Count < 2)
                {
                    throw new InvalidFormulaException(_formula, $"not enough operands to apply {operatorChar} operator");
                }
                var rightOperand = ExpressionsStack.Pop();
                ExpressionsStack.Peek().Combine(rightOperand, changeSign: operatorChar != '+');
            }
            /// <summary>
            /// Apply all operators until close parentheses.
            /// </summary>
            internal void ApplyParenthesis()
            {
                PushVariable();
                if (OperatorsStack.Count == 0 || !OperatorsStack.Contains('('))
                {
                    throw new InvalidFormulaException(_formula, "has parenthesis that wasn't opened");
                }
                while (OperatorsStack.Peek() != '(')
                {
                    ApplyTopOperator();
                }
                // pop '('
                OperatorsStack.Pop();
            }
            /// <summary>
            /// Checks need of adding nil element to transform -x to 0-x.
            /// </summary>
            /// <param name="currentChar"></param>
            internal void CheckAndAddNilElement(char currentChar)
            {
                if (VariableBuilder.Length != 0 || currentChar == '=')
                {
                    return;
                }

                // apply imitation only on formula start or after equal operator
                if (ExpressionsStack.Count == 0 || OperatorsStack.Count > 0 && OperatorsStack.Peek() == '=')
                {
                    // 0 imitation in start or after '=': -x => 0-x +x => 0+x
                    ExpressionsStack.Push(new VariableExpressionsGroup(new VariablesExpression()));
                }
            }
        }
    }
}
