using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryDataOutput
    {
        /// <summary>
        /// The value received from the database.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// The instance of the object the value should be assigned to.
        /// </summary>
        public object Instance { get; set; }
        
        public DataMapItem DatabaseColumn { get; set; }
    }
}
