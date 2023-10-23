using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests.DAL
{
    public record UniqueValue
    {
        public class UniqueValueMap : DataMap<UniqueValue>
        {
            public UniqueValueMap()
            {
                Table("UniqueValues");
                Id(x => x.Guid, "Guid").Assigned();
                Map(x => x.Value, "Value");
            }
        }

        private UniqueValue() { }

        public UniqueValue(Guid id, int value)
        {
            Guid = id;
            Value = value;
        }

        public Guid Guid { get; set; }
        public int Value { get; set; }
    }
}
