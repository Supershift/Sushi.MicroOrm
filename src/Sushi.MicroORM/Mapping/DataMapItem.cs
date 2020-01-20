using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Represents the mapping between a property or field and a database column.
    /// </summary>
    public class DataMapItem
    {
        /// <summary>
        /// The event that is triggered before the SQL statement is created allowing for applying change to the statement.
        /// </summary>
        public QueryResultHandler Reflection { get;set; }
        internal void OnReflection(object instance, DataMapItem item, object value)
        {
            if (Reflection != null) Reflection(new QueryDataOutput() { Value = value, Instance = instance, DatabaseColumn = item});
        }
        internal bool HasReflection
        {
            get { return (Reflection != null); }
        }
        public PropertyInfo Instance { get; set; }

        internal string ColumnName
        {
            get
            {
                if (string.IsNullOrEmpty(this.Alias))
                    return Column;
                return Alias;
            }
        }

        internal string ColumnAlias
        {
            get
            {
                if (string.IsNullOrEmpty(this.Alias))
                    return Column;
                return $"{Column} AS {Alias}";
            }
        }

        /// <summary>
        /// Gets ot set the sending datamap entity.
        /// </summary>
        public DataMap Sender { get; set; }
        /// <summary>
        /// Gets or set the column mapped to this item.
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// Gets or set the alias of the mapped column.
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Gets or sets <see cref="PropertyInfo"/> about the mapped field or property.
        /// </summary>
        public PropertyInfo Info { get; set; }                
        /// <summary>
        /// Gets or sets a value indicating if the mapped column can be modified. If set to true, UPDATE and INSERT statements will not modify the column.
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// Gets or sets a value indicating if the mapped column is part of a primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// Gets or sets a value indicating if the mapped column is an identity column.
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// Gets or sets the length of the mapped column. This can be used to explicitly specify the length of a mapped VARCHAR or NVARCHAR column.
        /// </summary>
        public int Length { get; set; }        
        /// <summary>
        /// Gets or sets the <see cref="SqlDbType"/> of the mapped column.
        /// </summary>
        public SqlDbType SqlType { get; set; }
        /// <summary>
        /// Map setting for joined objects
        /// </summary>
    }
}
