using CanonicalForm.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    public class ParenthesisRemoverTests
    {
        private CanonicalForm.Core.Interfaces.IParenthesisRemover _remover;

        public ParenthesisRemoverTests()
        {
            _remover = new PolandNotaionParenthesisRemover(Extensions.CreatePool());
        }

        [Test]
        public void NullString_ThtowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _remover.RemoveParenthesis(null));
        }

        [Test]
        [TestCase("(x)", true)]
        [TestCase("-(x)", false)]
        [TestCase("(-(x))", false)]
        [TestCase("-(-(x))", true)]
        [TestCase("-((-(x)))", true)]
        [TestCase("-(x)", false)]
        public void RightSignAfterOpening(string formula, bool isPositive)
        {
            var result = _remover.RemoveParenthesis(formula);
            Assert.AreEqual(isPositive, result[0] != '-');
        }

        [Test]
        public void MultipleParenthesis_ShouldNotUsePreviousSign()
        {
            var result = _remover.RemoveParenthesis("-((-(x)))");
            Assert.AreEqual("x", result);
        }
    }
}
