using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Represents a parameter used in a SQL statement.
    /// </summary>
    public class SqlStatementParameter
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementParameter"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="length"></param>
        public SqlStatementParameter(string name, object value, SqlDbType type, int length) : this(name, value, type, length, null) { }

        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementParameter"/>.
        /// </summary>
        public SqlStatementParameter(string name, object value, SqlDbType type, int length, string typeName)
        {
            Name = name;
            Value = value;
            Type = type;
            Length = length;
            TypeName = typeName;
        }

        /// <summary>
        /// Gets or sets the name of the parameter, ie. @myParameter.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="SqlDbType"/> of the parameter. The <see cref="Value"/> must be compatible with this type.
        /// </summary>
        public SqlDbType Type { get; set; }
        /// <summary>
        /// Gets or sets the length of the parameter. This is optional in most cases.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// Gets or sets a custom type name. Can be used for user defined types.
        /// </summary>
        public string TypeName { get; set; }
    }
}
