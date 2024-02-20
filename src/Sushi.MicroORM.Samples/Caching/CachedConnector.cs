using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Sushi.MicroORM.Samples.Caching
{
    /// <summary>
    /// Provides a very basic implementation for a connector with cache. The scope of the cache is limited to an instance of the connector. Please mind that connectors are not thread-safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedConnector<T> : Connector<T> where T : new()
    {
        /// <summary>
        /// Gets a very simple sample of a cache
        /// </summary>
        private ConcurrentDictionary<string, object> Cache { get; } = new ConcurrentDictionary<string, object>();

        public override SqlStatementResult<TResult> ExecuteSqlStatement<TResult>(SqlStatement<T> statement)
        {
            SqlStatementResult<TResult> result = null;

            //does this operation interact with the cache?
            switch (statement.DMLStatement)
            {
                case DMLStatementType.Delete:
                case DMLStatementType.Insert:
                case DMLStatementType.Update:
                case DMLStatementType.InsertOrUpdate:
                    //perform the operation
                    result = base.ExecuteSqlStatement<TResult>(statement);

                    //force a cache flush for all cached objects that have a matching first part of the key (= all objects for same type)
                    var firstKeyPart = typeof(T).FullName + "_";
                    var keysToDelete = Cache.Keys.Where(x => x.StartsWith(firstKeyPart));
                    foreach (var keyToDelete in keysToDelete)
                        Cache.TryRemove(keyToDelete, out object val);

                    break;

                case DMLStatementType.Select:
                    //check if in cache, return if found
                    string key = GenerateKey(statement);
                    if (Cache.TryGetValue(key, out object cachedValue))
                    {
                        if (cachedValue is SqlStatementResult<TResult>)
                            result = (SqlStatementResult<TResult>)cachedValue;
                    }

                    //if not, perform the query and cache result
                    if (result == null)
                    {
                        result = base.ExecuteSqlStatement<TResult>(statement);
                        Cache.TryAdd(key, result);
                    }
                    break;

                default:
                    result = base.ExecuteSqlStatement<TResult>(statement);
                    break;
            }

            return result;
        }

        private string GenerateKey(SqlStatement<T> statement)
        {
            //use the type name of the mapped class as first key part
            //this will allow us to clear all objects in the cache with the same type
            var firstKeyPart = typeof(T).FullName;

            //hash the query and parameters
            var query = statement.GenerateSqlStatement();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                //hash query
                var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query));
                var secondKeyPart = Convert.ToBase64String(hashedBytes);

                //hash the parameters
                var sb = new StringBuilder();
                foreach (var parameter in statement.Parameters)
                    sb.Append(parameter.Name).Append(parameter.Value);
                string parameters = sb.ToString();
                hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(parameters));
                var thirdKeyPart = Convert.ToBase64String(hashedBytes);

                //build the key
                return $"{firstKeyPart}_{secondKeyPart}_{thirdKeyPart}";
            }
        }
    }
}