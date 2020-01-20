using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// The query data argument 
    /// </summary>
    public class QueryData
    {
        /// <summary>
        /// Gets or set the original requesting datamap.
        /// </summary>
        public DataMap Map { get; set; }
        /// <summary>
        /// Gets or sets the object that holds the sql statement.
        /// </summary>
        public Query Query { get; set; }
    }
}