using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Provides utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Determines the best matching <see cref="SqlDbType"/> for <paramref name="type"/>.
        /// </summary>        
        /// <returns></returns>
        public static SqlDbType GetSqlDbType(Type type)
        {
            //if this is a nullable type, we need to get the underlying type (ie. int?, float?, guid?, etc.)            
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                type = underlyingType;

            //if this is an enum, get the underlying type (by default it is int32, but could be different)
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();            

            //now assume sqldbtype based on CLR type
            var sqlDbType = SqlDbType.NVarChar;
            if (type == typeof(int))
                sqlDbType = SqlDbType.Int;
            else if (type == typeof(long))
                sqlDbType = SqlDbType.BigInt;
            else if (type == typeof(short))
                sqlDbType = SqlDbType.SmallInt;
            else if (type == typeof(byte))
                sqlDbType = SqlDbType.TinyInt;
            else if (type == typeof(bool))
                sqlDbType = SqlDbType.Bit;
            else if (type == typeof(Guid))
                sqlDbType = SqlDbType.UniqueIdentifier;
            else if (type == typeof(DateTime))
                sqlDbType = SqlDbType.DateTime;
            else if (type == typeof(DateTimeOffset))
                sqlDbType = SqlDbType.DateTimeOffset;
            else if (type == typeof(DateOnly))
                sqlDbType = SqlDbType.Date;
            else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                sqlDbType = SqlDbType.Decimal;
            else if (type == typeof(TimeSpan) || type == typeof(TimeOnly))
                sqlDbType = SqlDbType.Time;
            else if (type == typeof(byte[]))
                sqlDbType = SqlDbType.VarBinary;
            

            return sqlDbType;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to an enumeration member if <paramref name="type"/> or its underlying <see cref="Type"/> is an <see cref="Enum"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? ConvertValueToEnum(object? value, Type type)
        {
            // if the type is an enum, we need to convert the value to the enum's type
            if (type.IsEnum && value != null)
            {
                value = Enum.ToObject(type, value);
            }

            return value;
        }

        internal static DataTable CreateDataTableFromMap(DataMap map, bool identityInsert)
        {
            //create a datatable based on table
            var dataTable = new DataTable(map.TableName);
            var primaryKey = new List<DataColumn>();
            foreach (var databaseColumn in map.Items)
            {
                //create a datacolumn for each column attribute that is not read only
                //datatable does not use the SqlDbTypes but instead uses internal mapping to map .Net types
                if (!databaseColumn.IsReadOnly)
                {
                    var dataColumn = new DataColumn();

                    dataColumn.AllowDBNull = true;

                    if (databaseColumn.IsIdentity && identityInsert == false)
                    {
                        dataColumn.AutoIncrement = true;
                    }

                    if (databaseColumn.IsPrimaryKey && dataColumn.AutoIncrement == false)
                        primaryKey.Add(dataColumn);

                    dataColumn.ColumnName = databaseColumn.Column;

                    //nullable types are not supported, the underlying type needs to be provided 
                    var type = ReflectionHelper.GetMemberType(databaseColumn.MemberInfoTree);
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    if (underlyingType != null)
                        type = underlyingType;
                    dataColumn.DataType = type;

                    if (databaseColumn.Length > 0 && dataColumn.DataType == typeof(string))
                        dataColumn.MaxLength = databaseColumn.Length;

                    dataTable.Columns.Add(dataColumn);
                }
            }
            //set the primary key for this table (composite primary keys are supported)
            dataTable.PrimaryKey = primaryKey.ToArray();

            return dataTable;
        }
    }
}
