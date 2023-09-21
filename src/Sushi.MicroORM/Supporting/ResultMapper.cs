using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
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
        public async Task<T?> MapToSingleResultAsync<T>(DbDataReader reader, DataMap<T> map, CancellationToken cancellationToken)
        {
            T? result;
            // read the first row from the result
            bool recordFound = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (recordFound)
            {
                // map the columns of the first row to the result, using the map
                try
                {
                    result = (T)Activator.CreateInstance(typeof(T), true)!;
                }
                catch(Exception e)
                {
                    throw new ArgumentException("Please use parameterless constructor.", typeof(T).Name);
                }

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
            var result = await MapToMultipleResultsScalarAsync<TResult>(reader, cancellationToken);
            if (result.Any())
                return result[0];
            else
                return default;
        }

        /// <summary>
        /// Maps all rows found in the first resultset of <paramref name="reader"/> to a collectiobn of objects of type <typeparamref name="T"/> using the provided <paramref name="map"/>.        
        /// If <paramref name="reader"/> contains a second resultset, it is expected to contain a scalar value that will be used to set <see cref="PagingData.NumberOfRows"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>             
        public async Task<QueryListResult<T>> MapToMultipleResultsAsync<T>(DbDataReader reader, DataMap<T> map, CancellationToken cancellationToken)
        {
            var result = new QueryListResult<T>();
            //read all rows from the first resultset
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                T instance = (T)Activator.CreateInstance(typeof(T))!;
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

            // get target type
            var targetType = typeof(TResult);
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                targetType = underlyingType;
            }

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                //read the first column of the first row
                var value = reader.GetValue(0);

                // map DbNull values and enums
                if (value == DBNull.Value)
                {
                    value = null;
                }
                else if (targetType.IsEnum)
                {
                    // if the target type is an enum, we need to convert the value to the enum's type
                    value = Utility.ConvertValueToEnum(value, targetType);
                }

                //does it have the correct type?
                if (value is TResult)
                    result.Add((TResult)value);
                else
                    result.Add(default(TResult));        
            }

            return result;
        }

        internal void SetResultValuesToObject<T, TResult>(IDataRecord reader, DataMap<T> map, TResult instance)
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

                        // convert DBNull to null
                        if (value == DBNull.Value)
                        {
                            value = null;
                        }

                        ReflectionHelper.SetMemberValue(item.MemberInfoTree, value, instance, _options.DateTimeKind);
                        break;
                    }
                }
            }
        }
    }
}
