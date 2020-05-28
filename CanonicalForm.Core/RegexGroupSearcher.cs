﻿using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using CanonicalForm.Core.Models;

namespace CanonicalForm.Core
{
    public class RegexGroupSearcher : IGroupsSearcher, IFormulaValidator
    {
        private Regex _groupsRegex = new Regex(Constants.GroupRegexPattern);
        private Regex _variableRegex = new Regex(Constants.VariableRegexPattern);
        private Regex _lineFormulaRegex = new Regex($"^({Constants.GroupRegexPattern})+$");

        ///<inheritdoc/>
        public IEnumerable<GroupModel> SearchGroups(string validatedFormula)
        {
            if (!Validate(validatedFormula))
            {
                throw new InvalidFormulaException(validatedFormula);
            }
            var result = _groupsRegex.Matches(validatedFormula);
            var sign = 1;
            foreach (Match item in result)
            {
                if (!item.Success)
                {
                    throw new Exception("Invalid formula");
                }

                var operatorValue = item.Groups["operator"].Value;
                var factor = item.Groups["factor"].Success ? double.Parse(item.Groups["factor"].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture) : 1;
                switch (operatorValue)
                {
                    case "=-":
                        sign = -1;
                        break;
                    case "=+":
                    case "=":
                        sign = -1; 
                        factor = -factor;
                        break;
                    case "-":
                        factor *= -sign;
                        break;
                    default:
                        factor *= sign;
                        break;
                }

                var variables = item.Groups["variables"].Value;
                var matchCollection = _variableRegex.Matches(variables);

                var (variable, power) = GenerateVariable(matchCollection);
                yield return new GroupModel
                {
                    Variable = variable,
                    MaxPower = power,
                    Factor = factor,
                };
            }
        }

        ///<inheritdoc/>
        public bool Validate(string formula)
        {
            return !string.IsNullOrWhiteSpace(formula) && formula.Split('=').Length == 2 && _lineFormulaRegex.IsMatch(formula);
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
