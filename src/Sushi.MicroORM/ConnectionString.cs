using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// A set of connection strings for a database.
    /// </summary>
    public record ConnectionString
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConnectionString"/>.
        /// </summary>        
        public ConnectionString(string primary, string? readOnly)
        {
            Primary = primary;
            ReadOnly = readOnly;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConnectionString"/> with only a <see cref="Primary"/> value.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ConnectionString(string value) => new ConnectionString(value);

        /// <summary>
        /// Gets the primary connection string, which can be used for reads and writes.
        /// </summary>
        public string Primary { get; private set; }

        /// <summary>
        /// Gets a read-only connection string, which can be used for reads only.
        /// </summary>
        public string? ReadOnly { get; private set; }
    }

    /// <summary>
    /// A set of connection strings for a Sql Azure database.
    /// </summary>
    public record SqlAzureConnectionString : ConnectionString
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConnectionString"/>.
        /// </summary>        
        public SqlAzureConnectionString(string primary, bool generateReadOnly) : base(primary, generateReadOnly ? GenerateReadOnly(primary) : null)
        {
            
        }

        private static string GenerateReadOnly(string connectionString) 
        {
            if (!connectionString.EndsWith(';')) connectionString += ';';
            connectionString  += "ApplicationIntent=ReadOnly;";
            return connectionString;
        }
    }
}
