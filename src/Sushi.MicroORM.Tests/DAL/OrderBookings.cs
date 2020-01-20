using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{
    public class Data<T> : DataMap where T : new()
    {
        public Data() : base(typeof(T))
        {

        }
    }

    //[DataMap(typeof(OrderBookingsMap))]    
    public class OrderBookings
    {
        public class OrderBookingsMap : DataMap<OrderBookings>
        {
            public OrderBookingsMap()
            {
                Table("cat_Orders");
                
                Id(x => x.ID, "Order_Key");
                Map(x => x.CustomerID, "Order_Customer_Key");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Comments, "Order_Comments");
                Map(x => x.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Created2, "Order_Created").ReadOnly();
                Map(x => x.Bookings, "Order_Customer_Key").ExtendOn("Booking_CustomerID", true);
                //Join2(x => x.)
                //var xx = new Data<OrderBookings>();
                //xx.Complete(x => x.ID, "test");


            }
        }

        public int ID { get; set; }
        public int CustomerID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }
        public List<Booking> Bookings { get; set; }

        public static List<OrderBookings> FetchAll(int customerID)
        {
            var connector = new Connector<OrderBookings>();
            var filter = connector.CreateDataFilter();
            
            filter.Add(x => x.CustomerID, customerID);
            
            var result = connector.FetchAll(filter);
            return result;
        }
    }
}
