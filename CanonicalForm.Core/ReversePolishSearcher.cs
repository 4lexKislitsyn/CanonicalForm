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

            var variableBuilder = _pool.Get();
            var expressionsStack = new Stack<VariableExpressionsGroup>();

            void PushVariable()
            {
                if (variableBuilder.Length > 0)
                {
                    var variablevalue = variableBuilder.ToString();
                    var variable = _expressionFactory.GetVariable(variablevalue);
                    if (variable == null)
                    {
                        throw new InvalidFormulaException(validatedFormula, $"cannot parse variable '{variablevalue}'");
                    }
                    expressionsStack.Push(new VariableExpressionsGroup(variable));
                    variableBuilder.Clear();
                }
            }

            void ApplyOperator(char operatorChar)
            {
                PushVariable();
                if (expressionsStack.Count == 1)
                {
                    if (operatorChar == '-')
                    {
                        expressionsStack.Peek().ChangeSign();
                    }
                    else if (operatorChar != '+')
                    {
                        throw new InvalidFormulaException(validatedFormula, $"cannot apply {operatorChar} operator for 1 operand");
                    }
                    return;
                }

                if (expressionsStack.Count < 2)
                {
                    throw new InvalidFormulaException(validatedFormula, $"not enough operands to apply {operatorChar} operator");
                }
                var rightOperand = expressionsStack.Pop();
                expressionsStack.Peek().Combine(rightOperand, changeSign: operatorChar != '+');
            }

            try
            {
                var polandResult = new List<string>();
                var operatorsStack = new Stack<char>();

                void ApplyParenthesis()
                {
                    PushVariable();
                    char tempOperator;
                    if (operatorsStack.Count == 0 || !operatorsStack.Contains('('))
                    {
                        throw new InvalidFormulaException(validatedFormula, "has parenthesis that wasn't opened");
                    }
                    while ((tempOperator = operatorsStack.Pop()) != '(')
                    {
                        ApplyOperator(tempOperator);
                    }
                }

                bool IsSeparateGroupsOperator(char op) => op == '(' || op == '=';

                validatedFormula = validatedFormula.Replace(" ", "");
                for (int i = 0; i < validatedFormula.Length; i++)
                {
                    var currentChar = validatedFormula[i];

                    void PushOperator(char operatorChar)
                    {
                        if (!IsSeparateGroupsOperator(operatorChar))
                        {
                            if (operatorsStack.Count != 0 && variableBuilder.Length == 0 && !IsSeparateGroupsOperator(operatorsStack.Peek()))
                            {
                                // -- +- 
                                throw new InvalidFormulaException(validatedFormula, $"operator at position {i} should follow variable");
                            }
                        }
                        PushVariable();
                        operatorsStack.Push(operatorChar);
                    }

                    if (currentChar == '=')
                    {
                        // (x=
                        var hasOpenedGroup = operatorsStack.Contains('(');
                        // x=x=
                        var hasAnotherEqualSign = operatorsStack.Contains('=');
                        // =x
                        var noLeftSideVariables = variableBuilder.Length == 0 && expressionsStack.Count == 0;
                        // x+=; length is important : x= (builder == x, operators stack is empty)
                        var hasOpenedOperator = variableBuilder.Length == 0 && operatorsStack.Count > 0;
                        if (hasOpenedGroup || hasAnotherEqualSign || noLeftSideVariables)
                        {
                            throw new InvalidFormulaException(validatedFormula, "'=' operator should be single and follow ')' or variable");
                        }
                    }

                    switch (currentChar)
                    {
                        case '=':
                        case '-' when i == 0 || validatedFormula[i - 1] != '^':
                        case '+':
                            {
                                if (variableBuilder.Length == 0 && currentChar != '=')
                                {
                                    // 0 imitation for : =-x => =0-x =+x => =0+x
                                    expressionsStack.Push(new VariableExpressionsGroup(new VariablesExpression()));
                                }
                                if (operatorsStack.Count > 0)
                                {
                                    var prevOperator = operatorsStack.Peek();
                                    if (prevOperator == '-' || prevOperator == '+')
                                    {
                                        ApplyOperator(operatorsStack.Pop());
                                    }
                                }
                                PushOperator(currentChar);
                            }
                            break;
                        case '(':
                            PushOperator(currentChar);
                            break;
                        case ')':
                            ApplyParenthesis();
                            break;
                        default:
                            variableBuilder.Append(currentChar);
                            break;
                    }
                }
                while (operatorsStack.Count > 0)
                {
                    var operatorChar = operatorsStack.Pop();
                    if (operatorChar == '(')
                    {
                        throw new InvalidFormulaException(validatedFormula, "all parentheses must be closed");
                    }
                    ApplyOperator(operatorChar);
                }
                if (expressionsStack.Count > 1)
                {
                    throw new InvalidFormulaException(validatedFormula, "cannot transform formula to single expression");
                }
                return expressionsStack.Pop().Expressions.Where(x=> x.Factor != 0);
            }
            finally
            {
                _pool.Return(variableBuilder);
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

        private class VariableExpressionsGroup
        {
            public VariableExpressionsGroup(VariablesExpression expression)
            {
                Expressions.Add(expression);
            }

            public List<VariablesExpression> Expressions { get; } = new List<VariablesExpression>();

            public void Combine(VariableExpressionsGroup group, bool changeSign)
            {
                if (changeSign)
                {
                    group.ChangeSign();
                }
                Expressions.AddRange(group.Expressions);
                group.Expressions.Clear();
                if (Expressions.Count > 0 && Expressions[0].Factor == 0)
                {
                    Expressions.RemoveAt(0);
                }
            }

            public void ChangeSign() => Expressions.ForEach(x => x.Factor *= -1);
        }
    }
}
