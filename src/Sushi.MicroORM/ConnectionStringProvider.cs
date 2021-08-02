using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Provides and manages database connectionstrings
    /// </summary>
    public class ConnectionStringProvider
    {
        private string _defaultConnectionString;
        /// <summary>
        /// Gets or sets the default connection string which will be used if no type specific connection string is found.
        /// </summary>
        public string DefaultConnectionString
        {
            get
            {
                return _defaultConnectionString;
            }
            set
            {
                _defaultConnectionString = value;
                CachedConnectionStrings.Clear();
            }
        }

        /// <summary>
        /// Gets a collection of connection strings per typename.
        /// </summary>
        protected ConcurrentDictionary<string, string> MappedConnectionStrings { get; } = new ConcurrentDictionary<string, string>();
        
        /// <summary>
        /// Gets a collection of connection strings per resolved typename.
        /// </summary>
        protected ConcurrentDictionary<Type, string> CachedConnectionStrings { get; } = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Adds or updates the connection string for the specified typename.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="connectionString"></param>
        public void AddConnectionString(string typeName, string connectionString)
        {
            // add the connection string to the backing store            
            MappedConnectionStrings[typeName] = connectionString;

            // clear cache 
            CachedConnectionStrings.Clear();
        }

        /// <summary>
        /// Gets the database connection string for the specific type, based on mapped connection strings. If no connection strings were mapped or no mapped result was found the default connection string is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetConnectionString(Type type)
        {
            if (MappedConnectionStrings.Count > 0)
            {
                var useCaching = DatabaseConfiguration.IsConnectionStringCachingEnabled;
                //check if we already cached a connection string for this type
                if (useCaching)
                {
                    if (CachedConnectionStrings.ContainsKey(type))
                        return CachedConnectionStrings[type];
                }

                string typeName = type.ToString();

                //split the type name in parts
                var splitName = typeName.Split('.').ToList();

                //find the most specific match
                //first we search for the fully qualified name. if nothing found, we search for the name minus one part, etc.
                string connectionString = DefaultConnectionString;
                while (splitName.Count > 0)
                {
                    string searchPattern = string.Join(".", splitName);

                    //if the pattern is found, return the mapped connection string
                    if (MappedConnectionStrings.ContainsKey(searchPattern))
                    {
                        connectionString =  MappedConnectionStrings[searchPattern];
                        break;
                    }
                    //make the search pattern one part less specific
                    splitName.RemoveAt(splitName.Count - 1);
                }

                //cache result
                if(useCaching)
                {
                    CachedConnectionStrings[type] = connectionString;                    
                }
                return connectionString;
            }            
            
            return DefaultConnectionString;
        }
    }
}
