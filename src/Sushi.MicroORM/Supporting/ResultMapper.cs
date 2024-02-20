using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;

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
        public static T MapToSingleResult<T>(SqlDataReader reader, DataMap<T> map, FetchSingleMode fetchSingleMode) where T : new()
        {
            var instance = new T();
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
                switch (fetchSingleMode)
                {
                    case FetchSingleMode.ReturnDefaultWhenNotFound:
                        instance = default(T);
                        break;

                    case FetchSingleMode.ReturnNewObjectWhenNotFound:
                        break;
                }
            }

            return instance;
        }

        /// <summary>
        /// Maps the first row found in <paramref name="reader"/> to an object of type <typeparamref name="TResult"/>
        /// </summary>
        /// <param name="reader"></param>
        public static TResult MapToSingleResultScalar<TResult>(SqlDataReader reader)
        {
            //read the first row from the result
            bool recordFound = reader.Read();
            if (recordFound)
            {
                //read the first column of the first row
                var value = reader.GetValue(0);
                //does it have the correct type?
                if (value is TResult)
                    return (TResult)value;
            }

            return default(TResult);
        }

        /// <summary>
        /// Maps all rows found in the first resultset of <paramref name="reader"/> to a collectiobn of objects of type <typeparamref name="T"/> using the provided <paramref name="map"/>.
        /// If <paramref name="reader"/> contains a second resultset, it is expected to contain a scalar value that will be used to set <see cref="PagingData.NumberOfRows"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="map"></param>
        public static List<T> MapToMultipleResults<T>(SqlDataReader reader, DataMap<T> map) where T : new()
        {
            var result = new List<T>();
            //read all rows from the first resultset
            while (reader.Read())
            {
                T instance = new T();
                SetResultValuesToObject(reader, map, instance);
                result.Add(instance);
            }

            return result;
        }

        /// <summary>
        /// Converts the first column of all rows found in <paramref name="reader"/> to an object of type <typeparamref name="TResult"/>
        /// </summary>
        /// <param name="reader"></param>
        public static QueryListResult<TResult> MapToMultipleResultsScalar<TResult>(SqlDataReader reader)
        {
            var result = new QueryListResult<TResult>();
            while (reader.Read())
            {
                //read the first column of the first row
                var value = reader.GetValue(0);
                //does it have the correct type?
                if (value is TResult)
                    result.Add((TResult)value);
                else
                    result.Add(default(TResult));
            }

            return result;
        }

        private static TResult SetResultValuesToObject<T, TResult>(SqlDataReader reader, DataMap<T> map, TResult instance) where T : new() where TResult : new()
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