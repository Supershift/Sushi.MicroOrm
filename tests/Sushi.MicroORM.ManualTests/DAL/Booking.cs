using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL
{
    [DataMap(typeof(BookingMap))]
    public class Booking
    {
        public class BookingMap : DataMap<Booking>
        {
            public BookingMap()
            {
                Table("cat_Bookings");
                Id(x => x.ID, "Booking_Key");
                Map(x => x.CustomerID, "Booking_CustomerID");
            }
        }

        public int ID { get; set; }
        public int CustomerID { get; set; }
    }
}