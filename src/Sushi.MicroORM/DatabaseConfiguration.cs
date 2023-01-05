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
    /// Specifies the options available for fetching single records
    /// </summary>
    public enum FetchSingleMode
    {
        /// <summary>
        /// Returns the Default for a class when no record found (which is in most cases NULL)
        /// </summary>
        ReturnDefaultWhenNotFound,
        /// <summary>
        /// Returns a new instance for a class when no record found
        /// </summary>
        ReturnNewObjectWhenNotFound
    }

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
        /// Gets or sets the behavior for connector's FetchSingle methods in case a record is not found in the database. 
        /// Default behavior is ReturnDefaultWhenNotFound.
        /// </summary>
        [Obsolete("Use Connector.FetchSingleMode")]
        public static FetchSingleMode FetchSingleMode { get; set; } = FetchSingleMode.ReturnDefaultWhenNotFound;

        /// <summary>
        /// Gets or sets an action that is called on every command sent to a database. It is advised to only use this in debug.
        /// </summary>
        public static Action<string> Log { get; set; }
    }
}
