using CanonicalForm.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using CanonicalForm.Core.Models;

namespace CanonicalForm.Core
{
    public class RegexGroupSearcher : IExpressionSearcher, IFormulaValidator
    {
        private readonly IVariableExpressionFactory _expressionFactory;
        private readonly Regex _groupsRegex = new Regex(Constants.GroupRegexPattern);
        private readonly Regex _lineFormulaRegex = new Regex($"^({Constants.GroupRegexPattern})+$");

        public RegexGroupSearcher(IVariableExpressionFactory expressionFactory)
        {
            _expressionFactory = expressionFactory;
        }

        ///<inheritdoc/>
        public IEnumerable<VariablesExpression> SearchGroups(string validatedFormula)
        {
            if (!Validate(validatedFormula))
            {
                throw new InvalidFormulaException(validatedFormula);
            }
            var result = _groupsRegex.Matches(validatedFormula);
            var sign = 1;
            foreach (Match item in result)
            {
                var variable = _expressionFactory.GetVariable(item.Value);

                var operatorValue = item.Groups["operator"].Value;
                switch (operatorValue)
                {
                    case "=-":
                        sign = -1;
                        break;
                    case "=+":
                    case "=":
                        sign = -1;
                        variable.Factor = -variable.Factor;
                        break;
                    case "-":
                        variable.Factor *= -sign;
                        break;
                    default:
                        variable.Factor *= sign;
                        break;
                }

                yield return variable;
            }
        }

        ///<inheritdoc/>
        public bool Validate(string formula)
        {
            return !string.IsNullOrWhiteSpace(formula) && formula.Split('=').Length == 2 && _lineFormulaRegex.IsMatch(formula);
        }
    }
}
