using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class TestFake
    {
        public class TestFakeMap : DataMap<TestFake>
        {
            public TestFakeMap()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name");
            }
        }
        
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public enum TestEnum
    {
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }
}
