using CanonicalForm.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CanonicalForm.Core
{
    /// <summary>
    /// Class that transform string to <see cref="VariablesExpression"/> by regex expression.
    /// </summary>
    public class RegexVariableExpressionFactory : Interfaces.IVariableExpressionFactory
    {
        private Regex _variableRegex = new Regex(Constants.VariableRegexPattern);
        private Regex _unsignedExpressionRegex = new Regex(Constants.VariableExpressionPattern);

        /// <inheritdoc/>
        public VariablesExpression GetVariable(string variableValue)
        {
            var item = _unsignedExpressionRegex.Match(variableValue);
            if (!item.Success)
            {
                throw new ArgumentOutOfRangeException(nameof(variableValue), variableValue, "Cannot parse variable");
            }

            var factor = item.Groups["factor"].Success ? double.Parse(item.Groups["factor"].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture) : 1;

            var variables = item.Groups["variables"].Value;
            var matchCollection = _variableRegex.Matches(variables);

            var (variable, power) = GenerateVariable(matchCollection);
            return new VariablesExpression
            {
                Variable = variable,
                MaxPower = power,
                Factor = factor,
            };
        }

        /// <summary>
        /// Get sorted expressions and max power of the inner variables.
        /// </summary>
        /// <param name="variablesMatch"></param>
        /// <returns></returns>
        private (string variable, int maxPower) GenerateVariable(MatchCollection variablesMatch)
        {
            var variables = new SortedList<string, VariableInfo>(variablesMatch.Count);
            var maxPower = int.MinValue;
            foreach (Match item in variablesMatch)
            {
                var power = item.Groups["pow"].Success ? int.Parse(item.Groups["pow"].Value) : 1;
                var name = power == 0 ? string.Empty : item.Groups["variable"].Value;
                if (power > maxPower)
                {
                    maxPower = power;
                }
                if (variables.ContainsKey(name))
                {
                    var variable = variables[name];
                    variable.Power += power;
                    if (variable.Power > maxPower)
                    {
                        maxPower = variable.Power;
                    }
                    variables[name] = variable;
                }
                else
                {
                    variables.Add(name, new VariableInfo(name, power));
                }
            }
            return (string.Join("", variables.Values), maxPower);
        }

        struct VariableInfo : IComparable<VariableInfo>
        {
            public string Name;
            public int Power;

            /// <summary>
            /// Create an instance of variable in expression.
            /// </summary>
            /// <param name="name">Name of the variable.</param>
            /// <param name="power">Power of the variable.</param>
            public VariableInfo(string name, int power)
            {
                Name = name;
                Power = power;
            }

            ///<inheritdoc/>
            public int CompareTo(VariableInfo other)
                => Name.CompareTo(other.Name);

            public override string ToString()
            {
                return Power > 1 ? $"{Name}^{Power}" : Name;
            }
        }
    }
}
