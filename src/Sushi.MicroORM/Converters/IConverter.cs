using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Converters
{
    /// <summary>
    /// Defines an interface to convert between database values and object properties.
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Converts the provided value, read from the database, to a value that can be assigned to a property.
        /// </summary>
        /// <param name="value">The value as provided by the database reader.</param>
        /// <param name="targetType">The type to which the value must be converted.</param>
        /// <returns></returns>
        object? FromDb(object? value, Type targetType);

        /// <summary>
        /// Converts the provided value, read from a property, to a value that can be written to the database.
        /// </summary>
        /// <param name="value">The value as provided by an object's property or field.</param>
        /// <param name="sourceType">The type of the value.</param>
        /// <returns></returns>
        object? ToDb(object? value, Type sourceType);
    }
}
