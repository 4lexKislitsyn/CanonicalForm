using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    public class RegexValidatorTests
    {
        [TestCase("x = x^2x")]
        [TestCase("x = x^x")]
        [TestCase("1 = 0")]
        [TestCase("x3.5")]
        [TestCase("x^-1")]
        public void InvalidFormulas(string formula)
        {

        }
    }
}
