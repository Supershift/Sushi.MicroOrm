using Sushi.MicroORM.Mapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
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

        public static List<BookedRoom> FetchAll(int customerID)
        {
            var connector = new Connector<BookedRoom>();
            var filter = connector.CreateDataFilter();
            filter.Add(x => x.CustomerID, customerID);
            var result = connector.FetchAll(filter);
            return result;
        }

        public static async Task<List<BookedRoom>> FetchAllAsync(int customerID)
        {
            var connector = new Connector<BookedRoom>();
            var filter = connector.CreateDataFilter();
            filter.Add(x => x.CustomerID, customerID);
            var result = await connector.FetchAllAsync(filter);
            return result;
        }
    }
}