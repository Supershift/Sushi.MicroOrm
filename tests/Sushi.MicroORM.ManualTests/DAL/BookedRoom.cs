using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests.DAL
{
    public class BookedRoom
    {
        public class BookedRoomMap : DataMap<BookedRoom>
        {
            public BookedRoomMap()
            {
                Table("cat_Bookings JOIN cat_Rooms ON Booking_Room_Number = Room_Number");
                Id(x => x.BookingID, "Booking_Key").Identity();
                Id(x => x.RoomNumber, "Room_Number").Identity();
                Map(x => x.RoomType, "Room_Type");
                Map(x => x.CustomerID, "Booking_CustomerID");
            }
        }

        public int BookingID { get; set; }
        public int RoomNumber { get; set; }
        public int RoomType { get; set; }
        public int CustomerID { get; set; }
    }
}
