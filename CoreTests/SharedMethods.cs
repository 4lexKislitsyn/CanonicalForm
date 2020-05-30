using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    internal static class SharedMethods
    {
        /// <summary>
        /// Instance of <see cref="DefaultObjectPoolProvider"/>
        /// </summary>
        internal static readonly ObjectPoolProvider PoolProvider = new DefaultObjectPoolProvider();
        /// <summary>
        /// Policy to create instance of <see cref="ObjectPool{T}"/> for <see cref="StringBuilder"/>.
        /// </summary>
        internal static readonly StringBuilderPooledObjectPolicy StringBuilderPolicy = new StringBuilderPooledObjectPolicy();
        /// <summary>
        /// Create instance of <see cref="ObjectPool{T}"/> using <see cref="PoolProvider"/> and <see cref="StringBuilderPolicy"/>.
        /// </summary>
        /// <returns></returns>
        public static ObjectPool<StringBuilder> CreatePool() => PoolProvider.Create(StringBuilderPolicy);
    }
}
