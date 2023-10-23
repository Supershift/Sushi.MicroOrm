using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests.DAL
{
    public record Parent
    {
        public class ParentMap : DataMap<Parent>
        {
            public ParentMap()
            {
                Table("Parents");
                Id(x => x.Id, "ParentId").Identity();
            }
        }

        public int Id { get; set; } 
    }
}
