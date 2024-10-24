using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Converters
{
    /// <summary>
    /// Converts objects to and from JSON strings. Requires the column's sql type to be a string/character type.
    /// </summary>
    public class JsonConverter : IConverter
    {
        /// <inheritdoc/>        
        public object? FromDb(object? value, Type targetType)
        {
            // get the value's string representation and deserialize it to the target type
            var json = value?.ToString();
            if (!string.IsNullOrWhiteSpace(json))
                return JsonSerializer.Deserialize(json, targetType);
            else
                return null;
        }

        /// <inheritdoc/>
        public object? ToDb(object? value, Type sourceType)
        {
            // serialize the value to json
            var result = JsonSerializer.Serialize(value, sourceType);
            return result;
        }
    }
}
