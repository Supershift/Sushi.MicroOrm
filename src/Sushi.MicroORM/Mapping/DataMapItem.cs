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
        /// Gets or sets <see cref="MemberInfo"/> about the mapped field or property.
        /// </summary>
        public List<MemberInfo> MemberInfoTree { get; } = new List<MemberInfo>();       
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
        /// Gets the statement to use in a select list. If <see cref="Alias"/> is defined, this will return '<see cref="Column"/> as <see cref="Alias"/>.
        /// Otherwiste only <see cref="Column"/> is returned.
        /// </summary>
        public string ColumnSelectListName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Alias))
                    return Column;
                return $"{Column} AS {Alias}";
            }
        }
    }
}
