using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalForm.Core
{
    internal static class Constants
    {
        //internal const string GroupRegexPattern = @"(?:^| ?(?'operator'\+|-|=) ?)(?'factor'\d+(?:[.,]\d+)?)?(?'variable'[a-zA-Z]+)(?:\^(?'power'\d+))?";
        internal const string VariableRegexPattern = @"(?<variable>[a-zA-Z])(?:\^(?<pow>-?\d+))?";
        internal static readonly string VariableExpressionPattern = $@"(?<factor>\d+(?:[.,]\d+)?)?(?<variables>({VariableRegexPattern})+)";
        internal static readonly string GroupRegexPattern = $@"(?:^| ?(?<operator>\+|-|=[-+]?) ?){VariableExpressionPattern}";
    }
}
