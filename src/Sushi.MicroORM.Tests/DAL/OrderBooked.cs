using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{
    [DataMap(typeof(OrderBookedMap))]    
    public class OrderBooked
    {
        public class OrderBookedMap : DataMap<OrderBooked>
        {
            public OrderBookedMap()
            {
                Table("cat_Orders A");
                Id(x => x.ID, "Order_Key");
                Map(x => x.CustomerID, "Order_Customer_Key");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Comments, "Order_Comments");
                Map(x => x.OrderCount, "(select count(*) from cat_Orders B where A.Order_Customer_Key = B.Order_Customer_Key)").Alias("Orders").ReadOnly();

                Map(x => x.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Created2, "Order_Created").ReadOnly();
                Map(x => x.Booking, "Order_Customer_Key").ExtendOn("Booking_CustomerID");
            }
        }

        public int ID { get; set; }
        public int CustomerID { get; set; }
        public int OrderCount { get; set; }

        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }

        public Booking Booking { get; set; }
        //public List<Booking> Booking { get; set; }

        public static List<Order> FetchAll(int customerID)
        {
            var connector = new Connector<Order>();

            var filter = connector.CreateDataFilter();
            
            filter.Add(x => x.CustomerID, customerID);
            
            var result = connector.FetchAll(filter);
            return result;
        }
    }
}
