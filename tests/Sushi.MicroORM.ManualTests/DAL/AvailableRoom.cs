using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL
{
    [DataMap(typeof(AvailableRoomMap))]
    public class AvailableRoom
    {
        public class AvailableRoomMap : DataMap<AvailableRoom>
        {
            public AvailableRoomMap()
            {
                Table("udf_GetAvailableRooms(@startDate, @endDate)");
                Id(x => x.Number, "Room_Number").Assigned();
                Map(x => x.Type, "Room_Type");
            }
        }


        public int Number { get; set; }
        public int Type { get; set; }        
    }
}
