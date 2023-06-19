using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Base class to define a profile of <see cref="DataMap"/> classes to use when resolving queries.
    /// </summary>
    public abstract class DataMapProfile
    {        
        private Dictionary<Type, Type> _dataMapTypes { get; } = new();

        /// <summary>
        /// Gets a collection of key value pairs with default relations between <see cref="DataMap"/> classes and mapped classes.
        /// </summary>
        public IReadOnlyDictionary<Type,Type> DataMapTypes { get { return _dataMapTypes; } }

        /// <summary>
        /// Sets the DataMap <typeparamref name="Y"/> to use when resolving queries for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Y"></typeparam>
        public void AddMapping<T, Y>() where T : new() where Y : DataMap<T>, new()
        {
            _dataMapTypes[typeof(T)] = typeof(Y);
        }
    }
}
