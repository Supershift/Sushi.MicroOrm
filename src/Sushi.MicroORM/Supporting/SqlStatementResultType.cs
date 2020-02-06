using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    public enum SqlStatementResultType
    {
        /// <summary>
        /// The statement returns one row.
        /// </summary>
        Single,
        /// <summary>
        /// The statement returns multiple rows.
        /// </summary>
        Multiple,
        /// <summary>
        /// The statement returns a scalar value.
        /// </summary>
        Scalar,
        /// <summary>
        /// The statement has no return value.
        /// </summary>
        None
    }
}
