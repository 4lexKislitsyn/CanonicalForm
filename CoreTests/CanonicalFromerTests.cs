using CanonicalForm.Core;
using CanonicalForm.Core.Interfaces;
using CanonicalForm.Core.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    public class CanonicalFromerTests
    {
        [Test]
        public void InvalidFormula_ReturnsNull()
        {
            var searcher = new Mock<IExpressionSearcher>();
            searcher.As<IFormulaValidator>()
                .Setup(x => x.Validate(It.IsAny<string>()))
                .Returns(false);

            var former = new CanonicalFormulaFormer(searcher.Object, Mock.Of<IExpressionsRenderer>());
            var result = former.Transform(string.Empty);
            Assert.IsNull(result);
        }

        [Test]
        public void InvalidFormula_DoesntCallSearch()
        {
            var searcher = new Mock<IExpressionSearcher>();
            searcher.As<IFormulaValidator>()
                .Setup(x => x.Validate(It.IsAny<string>()))
                .Returns(false)
                .Verifiable();

            var former = new CanonicalFormulaFormer(searcher.Object, Mock.Of<IExpressionsRenderer>());

            var result = former.Transform(string.Empty);
            Assert.IsNull(result);
            searcher.Verify();
            searcher.Verify(x => x.SearchGroups(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ValidFormula_CallSearch()
        {
            var searcher = new Mock<IExpressionSearcher>();
            searcher.As<IFormulaValidator>()
                .Setup(x => x.Validate(It.IsAny<string>()))
                .Returns(true)
                .Verifiable();

            var former = new CanonicalFormulaFormer(searcher.Object, Mock.Of<IExpressionsRenderer>());

            former.Transform(string.Empty);
            searcher.Verify();
            searcher.Verify(x => x.SearchGroups(It.IsAny<string>()), Times.AtLeastOnce());
        }

        [Test]
        public void SearcherNotValidator_CallSearch()
        {
            var searcher = new Mock<IExpressionSearcher>();

            var former = new CanonicalFormulaFormer(searcher.Object, Mock.Of<IExpressionsRenderer>());

            former.Transform(string.Empty);
            searcher.Verify(x => x.SearchGroups(It.IsAny<string>()), Times.AtLeastOnce());
        }

        [Test]
        public void NullGroups_DoentCallRenderer()
        {
            var renderer = new Mock<IExpressionsRenderer>();
            var searcher = new Mock<IExpressionSearcher>();
            searcher.Setup(x => x.SearchGroups(It.IsAny<string>()))
                .Returns((IEnumerable<VariablesExpression>)null);

            var former = new CanonicalFormulaFormer(searcher.Object, renderer.Object);

            former.Transform(string.Empty);
            renderer.Verify(x => x.Render(null), Times.Never());
        }

    }
}
