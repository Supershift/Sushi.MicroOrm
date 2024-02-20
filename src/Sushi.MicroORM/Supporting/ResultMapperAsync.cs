using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Mapping;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Provides methods to map the results from a <see cref="SqlDataReader"/> to objects, based on <see cref="DataMap"/>.
    /// </summary>
    public static class ResultMapperAsync
    {
        /// <summary>
        /// Maps the first row found in <paramref name="reader"/> to an object of type <typeparamref name="T"/> using the provided <paramref name="map"/>.
        /// </summary>
        public static async Task<T> MapToSingleResultAsync<T>(SqlDataReader reader, DataMap<T> map, FetchSingleMode fetchSingleMode, CancellationToken cancellationToken) where T : new()
        {
            var instance = new T();
            //read the first row from the result
            bool recordFound = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
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
        public static async Task<TResult> MapToSingleResultScalarAsync<TResult>(SqlDataReader reader, CancellationToken cancellationToken)
        {
            //read the first row from the result
            bool recordFound = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
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
        public static async Task<QueryListResult<T>> MapToMultipleResultsAsync<T>(SqlDataReader reader, DataMap<T> map, CancellationToken cancellationToken) where T : new()
        {
            var result = new QueryListResult<T>();
            //read all rows from the first resultset
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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
        public static async Task<QueryListResult<TResult>> MapToMultipleResultsScalarAsync<TResult>(SqlDataReader reader, CancellationToken cancellationToken)
        {
            var result = new QueryListResult<TResult>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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