using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Provides methods to read results from a <see cref="DbDataReader"/> to objects, based on <see cref="DataMap"/>.
    /// </summary>
    public class ResultMapper
    {
        private readonly MicroOrmOptions _options;

        /// <summary>
        /// Creates a new instance of <see cref="MicroOrmOptions"/>.
        /// </summary>
        /// <param name="options"></param>
        public ResultMapper(IOptions<MicroOrmOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Maps the first row found in <paramref name="reader"/> to an object of type <typeparamref name="T"/> using the provided <paramref name="map"/>.
        /// </summary>                  
        public async Task<T?> MapToSingleResultAsync<T>(DbDataReader reader, DataMap<T> map, CancellationToken cancellationToken) where T : new() 
        {
            T? result;
            // read the first row from the result
            bool recordFound = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (recordFound)
            {
                // map the columns of the first row to the result, using the map
                result = new T();
                SetResultValuesToObject(reader, map, result);
            }
            else
            {
                // return default                 
                result = default;
            }
                        
            return result;
        }

        /// <summary>
        /// Maps the first row found in <paramref name="reader"/> to an object of type <typeparamref name="TResult"/>
        /// </summary>                  
        public async Task<TResult?> MapToSingleResultScalarAsync<TResult>(DbDataReader reader, CancellationToken cancellationToken) 
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
        public async Task<QueryListResult<T>> MapToMultipleResultsAsync<T>(DbDataReader reader, DataMap<T> map, CancellationToken cancellationToken) where T : new()
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
        public async Task<QueryListResult<TResult?>> MapToMultipleResultsScalarAsync<TResult>(DbDataReader reader, CancellationToken cancellationToken)
        {
            var result = new QueryListResult<TResult?>();
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

        internal void SetResultValuesToObject<T, TResult>(IDataRecord reader, DataMap<T> map, TResult instance) where T : new() where TResult : new() 
        {
            if (instance == null) 
                throw new ArgumentNullException(nameof(instance));
            
            // for each mapped member on the instance, go through the result set and find a column with the expected name
            for (int i = 0; i < map.Items.Count; i++)
            {
                var item = map.Items[i];
                
                // which name is expected in the result set by the mapped item
                string mappedName = item.Column;
                if (!string.IsNullOrWhiteSpace(item.Alias))
                    mappedName = item.Alias;

                // find a column matching the mapped name
                for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
                {
                    // get the name of the column as returned by the database
                    var columnName = reader.GetName(columnIndex);

                    if (mappedName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var value = reader.GetValue(columnIndex);
                        ReflectionHelper.SetMemberValue(item.MemberInfoTree, value, instance, _options.DateTimeKind);
                        break;
                    }
                }
            }
        }
    }
}
