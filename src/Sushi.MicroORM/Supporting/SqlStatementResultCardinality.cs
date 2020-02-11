using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Specifies options for the cardinality of the result set expected to be created by a SQL statement.
    /// </summary>
    public enum SqlStatementResultCardinality
    {
        /// <summary>
        /// The statement returns one row.
        /// </summary>
        SingleRow,
        /// <summary>
        /// The statement returns multiple rows.
        /// </summary>
        MultipleRows,        
        /// <summary>
        /// The statement has no return value.
        /// </summary>
        None
    }
}
