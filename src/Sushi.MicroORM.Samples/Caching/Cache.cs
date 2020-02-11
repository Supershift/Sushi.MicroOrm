using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Samples.Caching
{
    public static class Cache
    {
        /// <summary>
        /// Gets an instance of <see cref="ConcurrentDictionary{string, object}"/>. This sample has some limitations, use a more advanced implementation
        /// like .NET's IMemoryCache in production scenario's.
        /// </summary>
        public static ConcurrentDictionary<string, object> Instance { get; } = new ConcurrentDictionary<string, object>();
    }
}
