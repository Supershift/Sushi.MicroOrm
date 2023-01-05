using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Provides methods and properties to configure the Sushi MicroORM
    /// </summary>
    public static class DatabaseConfiguration
    {           
        /// <summary>
        /// Gets or sets a value indicating if connetionstrings are cached for types. This is only used if multiple connection strings are provided through the AddMappedConnectionString method. Default value is true.
        /// </summary>
        public static bool IsConnectionStringCachingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets an action that is called on every command sent to a database. It is advised to only use this in debug.
        /// </summary>
        public static Action<string> Log { get; set; }
    }
}
