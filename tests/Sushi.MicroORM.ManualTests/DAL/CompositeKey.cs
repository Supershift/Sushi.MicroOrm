using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL
{
    [DataMap(typeof(CompositeKeyMap))]
    public class CompositeKey
    {
        public class CompositeKeyMap : DataMap<CompositeKey>
        {
            public CompositeKeyMap()
            {
                Table("cat_CompositeKeys");
                Id(x => x.FirstID, "FirstID").Assigned();
                Id(x => x.SecondID, "SecondID").Assigned();
                Map(x => x.SomeValue, "SomeValue");
            }
        }

        public int FirstID { get; set; }
        public int SecondID { get; set; }
        public string SomeValue { get; set; }
    }
}
