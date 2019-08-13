using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Provides methods to fluently configure a <see cref="DataMapItem"/>.
    /// </summary>
    public class DataMapItemSetter
    {
        internal DataMapItem _dbcol;
        internal DataMapItemSetter(DataMapItem dbcol)
        {
            _dbcol = dbcol;
        }

        /// <summary>
        /// Sets the <see cref="SqlDbType"/> of the mapped column.
        /// </summary>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public DataMapItemSetter SqlType(SqlDbType sqlType)
        {
            _dbcol.SqlType = sqlType;
            return this;
        }

        /// <summary>
        /// Sets the length of the mapped column. This can be used to explicitly set the length for VARCHAR and NVARCHAR columns.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public DataMapItemSetter Length(int length)
        {
            _dbcol.Length = length;
            return this;
        }        

        /// <summary>
        /// Sets the property to read-only, indicating the underlying column cannot be modified. UPDATE and INSERT statements will not alter the mapped column.
        /// </summary>
        /// <returns></returns>
        public DataMapItemSetter ReadOnly()
        {
            _dbcol.IsReadOnly = true;
            return this;
        }

        /// <summary>
        /// Sets a value indicating the underlying column is an identity column, assigned by the database on INSERT. This is the default for columns mapped with <see cref="DataMap{T}.Id(Expression{Func{T, object}}, string)"/>.
        /// </summary>        
        /// <returns></returns>
        public DataMapItemSetter Identity()
        {
            _dbcol.IsIdentity = true;
            return this;
        }

        /// <summary>
        /// Sets a value indicating the underlying column is assigned by the application. This is the default for columns mapped with <see cref="DataMap{T}.Map(Expression{Func{T, object}}, string)" />.
        /// </summary>        
        /// <returns></returns>
        public DataMapItemSetter Assigned()
        {
            _dbcol.IsIdentity = false;
            return this;
        }
    }

}
