using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests.DAL
{
    public record Child
    {
        public class ChildMap : DataMap<Child>
        {
            public ChildMap()
            {
                Table("Children");
                Id(x=>x.Id, "ChildId").Identity();
                Map(x => x.ParentId, "ParentId");
            }
        }
        
        public int Id { get; set; }
        public int ParentId { get; set; }
    }
}
