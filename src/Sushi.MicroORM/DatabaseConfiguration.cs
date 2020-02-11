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
        /// Sets the default database connection string to use when connecting to a database. 
        /// </summary>
        /// <param name="defaultConnectionString"></param>
        public static void SetDefaultConnectionString(string defaultConnectionString)
        {
            ConnectionStringProvider.DefaultConnectionString = defaultConnectionString;            
        }

        /// <summary>
        /// Add an alternative database connection string which is resolved on the fully qualified typename of the dataobject. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="connectionString"></param>
        public static void AddMappedConnectionString(Type type, string connectionString)
        {
            AddMappedConnectionString(type.ToString(), connectionString);
        }

        /// <summary>
        /// Add an alternative database connection string which is resolved on (part of) of the fully qualified typename of the dataobject. The most specific match is used as connection string. Matching is case sensitive.
        /// </summary>
        /// <param name="typeName">Fully qualified name of the type to match. Part of the name can also be provided.</param>
        /// <param name="connectionString"></param>
        public static void AddMappedConnectionString(string typeName, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentNullException(nameof(typeName), "cannot be null or whitespace");
            ConnectionStringProvider.AddConnectionString(typeName, connectionString);
        }
                
        /// <summary>
        /// Gets or sets a value indicating if connetionstrings are cached for types. This is only used if multiple connection strings are provided through the AddMappedConnectionString method. Default value is true.
        /// </summary>
        internal static bool IsConnectionStringCachingEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the behavior for connector's FetchSingle methods in case a record is not found in the database. 
        /// Default behavior is ReturnDefaultWhenNotFound.
        /// </summary>
        [Obsolete("Use Connector.FetchSingleMode")]
        public static FetchSingleMode FetchSingleMode { get; set; } = FetchSingleMode.ReturnDefaultWhenNotFound;
        
        /// <summary>
        /// Gets the provider for the data map.
        /// </summary>
        public static DataMapProvider DataMapProvider { get; } = new DataMapProvider();
        internal static ConnectionStringProvider ConnectionStringProvider { get; } = new ConnectionStringProvider();

        /// <summary>
        /// Gets or sets an action that is called on every command sent to a database. It is advised to only use this in debug.
        /// </summary>
        public static Action<string> Log { get; set; }
    }
}
