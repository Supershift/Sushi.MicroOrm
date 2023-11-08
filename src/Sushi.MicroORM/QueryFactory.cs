using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <inheritdoc/>    
    public class QueryFactory : IQueryFactory
    {
        private readonly DataMapProvider _dataMapProvider;

        /// <summary>
        /// Creates a new instance of <see cref="QueryFactory"/>.
        /// </summary>
        /// <param name="dataMapProvider"></param>
        public QueryFactory(DataMapProvider dataMapProvider)
        {
            _dataMapProvider = dataMapProvider;
        }

        /// <inheritdoc/>
        public DataQuery<T> CreateQuery<T>()
        {
            var map = _dataMapProvider.GetMapForType<T>();
            return new DataQuery<T>(map);
        }
    }
}
