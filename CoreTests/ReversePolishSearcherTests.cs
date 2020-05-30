using CanonicalForm.Core;
using CanonicalForm.Core.Interfaces;
using Microsoft.Extensions.ObjectPool;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreTests
{
    public class ReversePolishSearcherTests
    {
        private IExpressionSearcher searcher;

        [SetUp]
        public void InintSearcher()
        {
            searcher = new ReversePolishSearcher(SharedMethods.CreatePool(), new RegexVariableExpressionFactory());
        }

        [Test]
        public void NullString_ThrowsInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups(null).ToArray());
        }
        [Test]
        public void EmptyString_ThrowsInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups(string.Empty).ToArray());
        }

        [Test]
        public void WhitespaceString_ThrowsInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups(" ").ToArray());
        }

        [Test]
        public void NoEqualSign_ThrowsInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x+y").ToArray());
        }

        [Test]
        public void MultipleEqualSigns_ThrowsInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x=y=z").ToArray());
        }

        [Test]
        public void LeadingZeroPower_DoesntThrowsException()
        {
            var groups = searcher.SearchGroups("x^01=x").ToArray();
            Assert.AreEqual(2, groups.Length);
            Assert.IsTrue(groups.All(z => z.Variable == "x" && z.MaxPower == 1));
        }

        [TestCase("x=y")]
        [TestCase("x^0=y")]
        [TestCase("x=y^-1")]
        [TestCase("x=y^-1")]
        [TestCase("x^01=y")]
        [TestCase("x^1=y^1")]
        public void ValidFormula_DoesntThrowException(string formula)
        {
            Assert.DoesNotThrow(() => searcher.SearchGroups(formula).ToArray());
        }

        [TestCase("x=x", -1)]
        [TestCase("x=-x", 1)]
        public void RightSideVaribleTransfer_ChangeSign(string formula, int factor)
        {
            var groups = searcher.SearchGroups(formula).ToArray();
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual(factor, groups[1].Factor);
            Assert.AreEqual(factor, groups[1].Factor);
        }

        [Test]
        public void NillPower_ShouldBeTrimmed()
        {
            var groups = searcher.SearchGroups("x^0=y^0").ToArray();
            Assert.AreEqual(2, groups.Length);
            Assert.IsTrue(groups.All(x => x.Variable == string.Empty));
            Assert.AreEqual(0, groups.Sum(x => x.Factor));
        }

        [Test]
        [TestCase("-x^2 + 3.5xy + y = y^2 - xy + y", 6)]
        [TestCase("x^3- x^2y +x^-1y^2 + 3.5xy+y=y^2-xy+y", 8)]
        public void GroupsCountTest(string formula, int groupsCount)
        {
            var result = searcher.SearchGroups(formula).ToArray();
            Assert.AreEqual(groupsCount, result.Length);
        }
        [Test]
        public void OpenedParathesis_ThrowInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x=(y"));
        }
        [Test]
        public void NotOpenedCloseParathesis_ThrowInvalidFormulaException()
        {
            // empty operators stack
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x)=y"));
            // operators stack contains subtract operator
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("-x)=y"));
        }

        [Test]
        public void MultipleEqualSigns_ThrowInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x=y=z"));
        }

        [Test]
        public void OpenedOperatorFolloweByEqualSign_ThrowInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("x-=y"));
        }

        [Test]
        public void EqualSignLeadFormula_ThrowInvalidFormulaException()
        {
            Assert.Throws<InvalidFormulaException>(() => searcher.SearchGroups("=z"));
        }

        /// <summary>
        /// When transform -x to 0-x after parenthesis additional variable was added.
        /// </summary>
        [Test]
        public void SubstractAfterParenthesis_NillShouldNotBeAdded()
        {
            Assert.DoesNotThrow(() => searcher.SearchGroups("(x)-y=z"));
        }
    }
}