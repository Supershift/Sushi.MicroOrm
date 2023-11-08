using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Defines methods to create <see cref="DataQuery{T}"/> objects.
    /// </summary>    
    public interface IQueryFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="DataQuery{T}"/>. 
        /// </summary>
        /// <returns></returns>
        DataQuery<T> CreateQuery<T>();
    }
}
