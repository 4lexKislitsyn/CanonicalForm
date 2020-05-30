using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    internal static class SharedMethods
    {
        internal static readonly ObjectPoolProvider PoolProvider = new DefaultObjectPoolProvider();
        internal static readonly StringBuilderPooledObjectPolicy StringBuilderPolicy = new StringBuilderPooledObjectPolicy();

        public static ObjectPool<StringBuilder> CreatePool() => PoolProvider.Create(StringBuilderPolicy);
    }
}
