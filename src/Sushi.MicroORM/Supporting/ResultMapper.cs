using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Provides methods to map the results from a <see cref="SqlDataReader"/> to objects, based on <see cref="DataMap"/>.
    /// </summary>
    public static class ResultMapper
    {
        /// <summary>
        /// Maps the first row found in <paramref name="reader"/> to an object of type <typeparamref name="T"/> using the provided <paramref name="map"/>.        
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="map"></param>
        /// <returns>Returns a <see cref="SqlStatementResult{T}"/> object with <see cref="SqlStatementResultType.Single"/></returns>
        public static SqlStatementResult<T> MapToSingleResult<T>(SqlDataReader reader, DataMap<T> map) where T : new()
        {
            T instance = new T();
            //read the first row from the result
            bool recordFound = reader.Read();
            if (recordFound)
            {
                //map the columns of the first row to the instance, using the map
                SetResultValuesToObject(reader, map, instance);
            }
            else            
            {
                //return default or empty object, based on configuration
                //todo: move setting to connector or mapping level?
                switch (DatabaseConfiguration.FetchSingleMode)
                {
                    case FetchSingleMode.ReturnDefaultWhenNotFound:
                        instance = default(T);
                        break;
                    case FetchSingleMode.ReturnNewObjectWhenNotFound:
                        break;
                }
            }

            //create result object
            var result = new SqlStatementResult<T>(instance);
            return result;
        }

        /// <summary>
        /// Maps all rows found in the first resultset of <paramref name="reader"/> to a collectiobn of objects of type <typeparamref name="T"/> using the provided <paramref name="map"/>.        
        /// If <paramref name="reader"/> contains a second resultset, it is expected to contain a scalar value that will be used to set <see cref="PagingData.NumberOfRows"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="map"></param>
        /// <returns>Returns a <see cref="SqlStatementResult{T}"/> object with <see cref="SqlStatementResultType.Single"/></returns>
        public static SqlStatementResult<T> MapToMultipleResults<T>(SqlDataReader reader, DataMap<T> map) where T : new()
        {
            var results = new List<T>();
            //read all rows from the first resultset
            while (reader.Read())
            {
                T instance = new T();
                SetResultValuesToObject(reader, map, instance);
                results.Add(instance);                
            }

            //if we have a second result set, it is the filter's paging
            int? totalNumberOfRows = null;
            if (reader.NextResult())
            {
                if (reader.Read())
                {
                    var candidate = reader.GetValue(0);
                    totalNumberOfRows = (int)candidate;
                }
            }

            //create result object
            var result = new SqlStatementResult<T>(results, totalNumberOfRows);
            return result;
        }

        private static T SetResultValuesToObject<T>(SqlDataReader reader, DataMap<T> map, T instance) where T : new()
        {
            //for each mapped member on the instance, go through the result set and find a column with the expected name
            foreach (var item in map.Items)
            {
                for (int column = 0; column < reader.FieldCount; column++)
                {
                    //get the name of the column as returned by the database
                    var columnName = reader.GetName(column);
                    //which name is expected in the result set by the mapped item
                    string mappedName = item.Column;
                    if (!string.IsNullOrWhiteSpace(item.Alias))
                        mappedName = item.Alias;

                    if (mappedName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var value = reader.GetValue(column);
                        ReflectionHelper.SetMemberValue(item.MemberInfoTree, value, instance);
                        break;
                    }
                }
            }
            return instance;
        }
    }
}
