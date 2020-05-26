using CanonicalForm.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace CoreTests
{
    public class RegexSearcherTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NullValidatedString()
        {
            var searcher = new RegexGroupSearcher();
            Assert.Throws<ArgumentNullException>(() => searcher.SearchGroups(null), "Null is invalid formula.");
        }

        [Test]
        [TestCase("x^2 + 3.5xy + y = y^2 - xy + y", 6)]
        [TestCase("x^3 + x^2y + xy^2 + 3.5xy + y = y^2 - xy + y", 8)]
        public void GetValidGroups(string formula, int groupsCount)
        {
            var searcher = new RegexGroupSearcher();
            var result = searcher.SearchGroups(formula).ToArray();
            Assert.AreEqual(groupsCount, result.Length);
        }

        [Test]
        [TestCase("x^2x")]
        public void InvalidFormula(string formula)
        {
            var searcher = new RegexGroupSearcher();
            searcher.SearchGroups(formula).ToArray();
        }
    }
}