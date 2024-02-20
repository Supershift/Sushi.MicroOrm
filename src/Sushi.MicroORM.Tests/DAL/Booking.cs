﻿using Sushi.MicroORM.Mapping;

namespace Sushi.MicroORM.Tests.DAL
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