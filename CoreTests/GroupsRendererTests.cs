using CanonicalForm.Core;
using CanonicalForm.Core.Models;
using Microsoft.Extensions.ObjectPool;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreTests
{
    public class GroupsRendererTests
    {
        private readonly ObjectPool<StringBuilder> pool = SharedMethods.CreatePool();
        private GroupsRenderer renderer;

        [SetUp]
        public void InitRenderer()
        {
            renderer = new GroupsRenderer(pool);
        }

        [TestCase(new int[] { 0, -1, 1 })]
        [TestCase(new int[] { 0 })]
        [TestCase(new int[] { 1, -1 })]
        [TestCase(new int[] { 0, -1 })]
        [TestCase(new int[] { 0, 1 })]
        public void MultipleEmptyVaribales_ShouldBeCombined(int[] factors)
        {
            var groups = factors.Select(x=> new VariablesExpression { Factor = x, Variable = string.Empty });
            var result = RenderWithBaseAsserts(groups);
            Assert.AreEqual($"{groups.Sum(x => x.Factor)}=0", result);
        }

        [Test]
        public void NullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => renderer.Render(null));
        }

        [Test]
        public void SameVariable_ShoudleBeCombined()
        {
            var groups = new[]
            {
                new VariablesExpression{ Factor = 1, Variable = "x"  },
                new VariablesExpression{ Factor = 3, Variable = "x"  }
            };
            Assert.AreEqual("4x=0", RenderWithBaseAsserts(groups));
        }

        [Test]
        public void NillFactor_ShouldBeRemoved()
        {
            var groups = new[]
            {
                new VariablesExpression{ Factor = 0, Variable = "x"  },
            };
            Assert.AreEqual("0=0", RenderWithBaseAsserts(groups));
        }

        [Test]
        public void NillSumFactor_ShouldbeRemoved()
        {
            var groups = new[]
            {
                new VariablesExpression{ Factor = 1, Variable = "x" },
                new VariablesExpression{ Factor = -1, Variable = "x" }
            };
            Assert.AreEqual("0=0", RenderWithBaseAsserts(groups));
        }

        private string RenderWithBaseAsserts(IEnumerable<VariablesExpression> groups)
        {
            var result = renderer.Render(groups);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.IsTrue(result.EndsWith("=0"));
            return result;
        }
    }
}
