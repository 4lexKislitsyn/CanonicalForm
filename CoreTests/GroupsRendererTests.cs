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
        private readonly ObjectPool<StringBuilder> pool = Extensions.CreatePool();
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
            var random = new Randomizer(DateTime.UtcNow.Minute);
            var groups = factors.Select(x=> new GroupModel { Factor = x, Variable = string.Empty });
            var result = RenderWithBaseAsserts(groups);
            Assert.AreEqual($"{groups.Sum(x => x.Factor)}=0", result);
        }

        private string RenderWithBaseAsserts(IEnumerable<GroupModel> groups)
        {
            var result = renderer.Render(groups);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.IsTrue(result.EndsWith("=0"));
            return result;
        }
    }
}
