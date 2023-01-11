using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL
{
    [DataMap(typeof(IdentifierMap))]
    public class Identifier
    {
        public class IdentifierMap : DataMap<Identifier>
        {
            public IdentifierMap()
            {
                Table("cat_Identifiers");
                Id(x => x.GUID, "Identifier_GUID").Assigned();
                Map(x => x.Batch, "Identifier_Batch");
            }
        }

        public Guid GUID { get; set; }
        public Guid Batch { get; set; }
    }
}
